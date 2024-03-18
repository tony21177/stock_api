using stock_api.Service;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace stock_api.Utils
{
    public class AuthHelpers
    {
        private readonly IConfiguration Configuration;
        private readonly MemberService _memberService;
        private readonly CompanyService _companyService;
        private readonly ILogger<AuthHelpers> _logger;

        public AuthHelpers(IConfiguration configuration, MemberService memberService,CompanyService companyService, ILogger<AuthHelpers> logger)
        {
            this.Configuration = configuration;
            _memberService = memberService;
            _companyService = companyService;
            _logger = logger;
        }

        public string GenerateToken(MemberAndPermissionSetting memberAndPermissionSetting, int expireMinutes = 43200)
        {
            var issuer = Configuration.GetValue<string>("JwtSettings:Issuer");
            var signKey = Configuration.GetValue<string>("JwtSettings:SignKey");

            // Configuring "Claims" to your JWT Token
            var claims = new List<Claim>();

            // In RFC 7519 (Section#4), there are defined 7 built-in Claims, but we mostly use 2 of them.
            //claims.Add(new Claim(JwtRegisteredClaimNames.Iss, issuer));
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, memberAndPermissionSetting.Member.Account)); 
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // JWT ID

            // The "NameId" claim is usually unnecessary.
            //claims.Add(new Claim(JwtRegisteredClaimNames.NameId, userName));

            // This Claim can be replaced by JwtRegisteredClaimNames.Sub, so it's redundant.
            //claims.Add(new Claim(ClaimTypes.Name, userName));

            // TODO: You can define your "roles" to your Claims.
            claims.Add(new Claim(ClaimTypes.Role, memberAndPermissionSetting.Member.AuthValue.ToString()));
            claims.Add(new Claim("account", memberAndPermissionSetting.Member.Account));
            claims.Add(new Claim("userId", memberAndPermissionSetting.Member.UserId));
            claims.Add(new Claim("compId", memberAndPermissionSetting.CompanyWithUnit.CompId));
            claims.Add(new Claim("compName", memberAndPermissionSetting.CompanyWithUnit.Name));
            claims.Add(new Claim("unitId", memberAndPermissionSetting.CompanyWithUnit.UnitId));
            claims.Add(new Claim("unitName", memberAndPermissionSetting.CompanyWithUnit.UnitName));
            claims.Add(new Claim("permissions", JsonSerializer.Serialize(memberAndPermissionSetting.PermissionSetting)));
            claims.Add(new Claim("member", JsonSerializer.Serialize(memberAndPermissionSetting.Member)));

            var userClaimsIdentity = new ClaimsIdentity(claims);

            // Create a SymmetricSecurityKey for JWT Token signatures
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey??"KimForest"));

            // HmacSha256 MUST be larger than 128 bits, so the key can't be too short. At least 16 and more characters.
            // https://stackoverflow.com/questions/47279947/idx10603-the-algorithm-hs256-requires-the-securitykey-keysize-to-be-greater
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            // Create SecurityTokenDescriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                //Audience = issuer, // Sometimes you don't have to define Audience.
                //NotBefore = DateTime.Now, // Default is DateTime.Now
                //IssuedAt = DateTime.Now, // Default is DateTime.Now
                Subject = userClaimsIdentity,
                Expires = DateTime.Now.AddMinutes(expireMinutes),
                SigningCredentials = signingCredentials
            };

            // Generate a JWT securityToken, than get the serialized Token result (string)
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var serializeToken = tokenHandler.WriteToken(securityToken);

            return serializeToken;
        }


        public MemberAndPermissionSetting? GetMemberAndPermissionSetting(ClaimsPrincipal? user)
        {
            if (user == null) return null;
            var userIdentity = user.Identity;

            var userClaims = user.Claims;
            var permissionClaim = userClaims.FirstOrDefault(c => c.Type == "permissions");
            if (permissionClaim == null) return null;

            var account = userClaims.FirstOrDefault(c => c.Type == "account");
            var compId = userClaims.FirstOrDefault(c => c.Type == "compId");
            if (account == null||compId==null) return null;
            var member = _memberService.GetMemberByAccount(account.Value);
            if (member == null) return null;
            var compWithUnit = _companyService.GetCompanyWithUnit(compId.Value);
            if (compWithUnit == null) return null;

            string permissionSettingString = permissionClaim.Value;
            PermissionSetting? permissionSetting = null;
            try
            {
                permissionSetting = JsonSerializer.Deserialize<PermissionSetting>(permissionSettingString);
            }
            catch (Exception ex)
            {
                _logger.LogError("解析jwt permission錯誤:{ex}", ex.Message);
            }
            if (permissionSetting == null) return null;
            var roleClaim = user.FindFirstValue(ClaimTypes.Role);
            if (roleClaim == null) return null;
            short authValue = Convert.ToInt16(roleClaim);
            var memberAndPermissionSetting = new MemberAndPermissionSetting(member, permissionSetting, compWithUnit);

            return memberAndPermissionSetting;
        }
    }
}
