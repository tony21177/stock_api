using stock_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using stock_api.Common.IoC.Configuration.DI;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

//services cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        //policy.AllowAnyMethod();
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();

    });
});

// Add DBContext
//var serverVersion = new MySqlServerVersion(new Version(5, 7, 29));
//builder.Services.AddEntityFrameworkMySQL().AddDbContext<HandoverContext>(options =>
//{
//    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion);
//    options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
//           .EnableSensitiveDataLogging();
//}, ServiceLifetime.Scoped);

var serverVersion = new MySqlServerVersion(new Version(5, 7, 29));
builder.Services.AddEntityFrameworkMySql().AddDbContext<HandoverContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion);
    options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
           .EnableSensitiveDataLogging();
}, ServiceLifetime.Scoped);


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed. {context.Exception}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated.");
                return Task.CompletedTask;
            },
            // ... other event handlers
        };
        // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
        options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

            // 一般我們都會驗證 Issuer
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

            // 通常不太需要驗證 Audience
            ValidateAudience = false,
            //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

            // 一般我們都會驗證 Token 的有效期間
            ValidateLifetime = true,

            // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
            ValidateIssuerSigningKey = false,

            // "1234567890123456" 應該從 IConfiguration 取得
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")))
        };
    });

builder.Services.AddAuthorization().AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>(); ;


// AutoMapper
builder.Services.ConfigureMappings();


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
}); ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.ConfigureBusinessServices(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


app.Run();

