using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;
using MaiBackend.PublicApi.Consts;
using Org.BouncyCastle.Asn1.Ocsp;

namespace stock_api.Controllers.Validator
{
    public class CreateOrUpdatePurchaseFlowSettingValidator : AbstractValidator<CreateOrUpdatePurchaseFlowSettingRequest>
    {
        private readonly PurchaseFlowSettingService _purchaseFlowSettingService;
        private readonly MemberService _memberService;
        private readonly CompanyService _companyService;

        public CreateOrUpdatePurchaseFlowSettingValidator(ActionTypeEnum action, PurchaseFlowSettingService purchaseFlowSettingService, MemberService memberService, CompanyService companyService)
        {
            _purchaseFlowSettingService = purchaseFlowSettingService;
            _memberService = memberService;
            _companyService = companyService;
            ClassLevelCascadeMode = CascadeMode.Stop;

            if (action == ActionTypeEnum.Create)
            {
                RuleFor(x => x.FlowName).NotEmpty().WithMessage("flowName為必須");
                RuleFor(x => x.Sequence).NotEmpty().WithMessage("sequence為必須");
                RuleFor(x => x.Sequence).Must((request, sequence, context) => SequenceUnique(request, sequence)).WithMessage("sequence已存在");
                RuleFor(x => x.UserId).NotEmpty().WithMessage("userId為必須");
                RuleFor(x => x.IsActive).NotEmpty().WithMessage("isActive為必須");
                RuleFor(x => x.UserId).Must((request, userId, context) => BeValidUser(request,userId)).WithMessage("userId不存在");
            }
            if (action == ActionTypeEnum.Update)
            {
                RuleFor(x => x.FlowId).NotEmpty().WithMessage("flowId為必須");
                // RuleFor(x => x.Sequence).Must((request, sequence, context) => SequenceUniqueOrNull(request, sequence)).WithMessage("sequence已存在");
                RuleFor(x => x.UserId).Must((request, userId, context) => BeValidUserOrNull(request, userId)).WithMessage("userId不存在");
            }
        }

        private bool SequenceUnique(CreateOrUpdatePurchaseFlowSettingRequest request,int? sequence)
        {
            return !_purchaseFlowSettingService.IsSequenceExist(sequence.Value, request.CompId);
        }
        private bool SequenceUniqueOrNull(CreateOrUpdatePurchaseFlowSettingRequest request, int? sequence)
        {
            if (!sequence.HasValue) { return true; };
            return !_purchaseFlowSettingService.IsSequenceExist(sequence.Value, request.CompId);
        }
        private bool BeValidUser(CreateOrUpdatePurchaseFlowSettingRequest request, string? userId)
        {
            return _memberService.GetActiveMembersByUserIds(new List<string>() { userId }, request.CompId).Count() > 0;
        }

        private  bool BeValidUserOrNull(CreateOrUpdatePurchaseFlowSettingRequest request, string? userId)
        {
            if(userId==null) { return true; };
            return _memberService.GetActiveMembersByUserIds(new List<string>() { userId },request.CompId).Count() > 0;
        }
    }
}
