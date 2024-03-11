using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;
using MaiBackend.PublicApi.Consts;

namespace stock_api.Controllers.Validator
{
    public class CreateOrUpdateMemberValidator : AbstractValidator<CreateOrUpdateMemberRequest>
    {
        private readonly AuthLayerService _authLayerService;
        private readonly MemberService _memberService;

        public CreateOrUpdateMemberValidator(ActionTypeEnum action, AuthLayerService authLayerService, MemberService memberService)
        {
            if (action == ActionTypeEnum.Create)
            {
                RuleFor(x => x.Account).NotEmpty().WithMessage("account為必須");
                RuleFor(x => x.Account).Must(AccountUnique).WithMessage("account已存在");
                RuleFor(x => x.Password).NotEmpty().WithMessage("password為必須");
                RuleFor(x => x.DisplayName).NotEmpty().WithMessage("displayName為必須");
                RuleFor(x => x.IsActive).NotEmpty().WithMessage("isActive為必須");
                RuleFor(x => x.AuthValue).NotEmpty().WithMessage("authValue為必須");
                RuleFor(x => x.Uid).NotEmpty().WithMessage("uid為必須");
                RuleFor(x => x.Uid).Must(UidUnique).WithMessage("uID已存在");
            }
            if (action == ActionTypeEnum.Update)
            {
                RuleFor(x => x.UserID).NotEmpty().WithMessage("userId為必須");
            }
            _authLayerService = authLayerService;
            _memberService = memberService;

            ClassLevelCascadeMode = CascadeMode.Stop;
            // 驗證相對應的Workspace,Space,Folder,List是否存在
            RuleFor(x => x.AuthValue).Must(ExistAuthValue).WithMessage("此階層不存在");
        }

        private bool AccountUnique(string? account)
        {
            return _memberService.IsAccountNotExist(account!);
        }
        private bool UidUnique(string? uid)
        {
            return _memberService.IsUidNotExist(uid!);
        }

        private bool ExistAuthValue(short? authValue)
        {
            if (authValue == null) return false;
            var existAuthLayer = _authLayerService.GetByAuthValue(authValue.Value);
            return existAuthLayer != null;
        }

        private static bool BeValidValue(string? userId)
        {
            return userId != null;
        }
    }
}
