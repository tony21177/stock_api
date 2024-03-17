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
                RuleFor(x => x.CompId).NotEmpty().WithMessage("compId為必須");
            }
            if (action == ActionTypeEnum.Update)
            {
                RuleFor(x => x.UserId).NotEmpty().WithMessage("userId為必須");
            }
            _authLayerService = authLayerService;
            _memberService = memberService;

            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x).Must(ExistAuthValue).WithMessage("此階層不存在");
        }

        private bool AccountUnique(string? account)
        {
            return _memberService.IsAccountNotExist(account!);
        }

        private bool ExistAuthValue(CreateOrUpdateMemberRequest request)
        {
            if (request.AuthValue == null || request.CompId == null) return false;
            var existAuthLayer = _authLayerService.GetByAuthValue(request.AuthValue.Value, request.CompId);
            return existAuthLayer != null;
        }

        private static bool BeValidValue(string? userId)
        {
            return userId != null;
        }
    }
}
