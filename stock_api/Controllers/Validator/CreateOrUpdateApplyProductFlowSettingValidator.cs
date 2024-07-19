using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;
using MaiBackend.PublicApi.Consts;
using Org.BouncyCastle.Asn1.Ocsp;

namespace stock_api.Controllers.Validator
{
    public class CreateOrUpdateApplyProductFlowSettingValidator : AbstractValidator<CreateOrUpdateApplyProductFlowSettingRequest>
    {
        private readonly ApplyProductFlowSettingService _applyProductFlowSettingService;
        private readonly MemberService _memberService;
        private readonly CompanyService _companyService;
        private readonly GroupService _groupService;

        public CreateOrUpdateApplyProductFlowSettingValidator(ActionTypeEnum action, ApplyProductFlowSettingService applyProductFlowSettingService,
            MemberService memberService, CompanyService companyService, GroupService groupService)
        {
            _applyProductFlowSettingService = applyProductFlowSettingService;
            _memberService = memberService;
            _companyService = companyService;
            _groupService = groupService;
            ClassLevelCascadeMode = CascadeMode.Stop;

            if (action == ActionTypeEnum.Create)
            {
                RuleFor(x => x.FlowName).NotEmpty().WithMessage("flowName為必須");
                RuleFor(x => x.Sequence).NotEmpty().WithMessage("sequence為必須");
                RuleFor(x => x.Sequence).Must((request, sequence, context) => SequenceUnique(request, sequence.Value)).WithMessage("sequence已存在");
                RuleFor(x => x).Custom((x, context) =>
                {
                    if (string.IsNullOrEmpty(x.ReviewUserId) && string.IsNullOrEmpty(x.ReviewGroupId))
                    {
                        context.AddFailure("", "ReviewUserId 和 ReviewGroupId 其中一個必須存在");
                    }
                });
                RuleFor(x => x.IsActive).NotEmpty().WithMessage("isActive為必須");
                RuleFor(x => x.ReviewUserId).Must((request, userId, context) => BeValidUser(request, userId))
                    .When(x=>x.ReviewUserId!=null).WithMessage("reviewUserId不存在");
                RuleFor(x => x.ReviewGroupId).Must((request, groupId, context) => BeValidGroup(request, groupId))
                    .When(x => x.ReviewGroupId != null).WithMessage("reviewGroupId不存在");
            }
            if (action == ActionTypeEnum.Update)
            {
                RuleFor(x => x.SettingId).NotEmpty().WithMessage("settingId為必須");
                RuleFor(x => x.ReviewUserId).Must((request, userId, context) => BeValidUser(request, userId))
                    .When(x => x.ReviewUserId != null).WithMessage("reviewUserId不存在");
                RuleFor(x => x.ReviewGroupId).Must((request, groupId, context) => BeValidGroup(request, groupId))
                    .When(x => x.ReviewGroupId != null).WithMessage("reviewGroupId不存在");
            }
            _groupService = groupService;
        }

        private bool SequenceUnique(CreateOrUpdateApplyProductFlowSettingRequest request,int sequence)
        {
            return !_applyProductFlowSettingService.IsSequenceExist(sequence, request.CompId);
        }
        private bool SequenceUniqueOrNull(CreateOrUpdatePurchaseFlowSettingRequest request, int? sequence)
        {
            if (!sequence.HasValue) { return true; };
            return !_applyProductFlowSettingService.IsSequenceExist(sequence.Value, request.CompId);
        }
        private bool BeValidUser(CreateOrUpdateApplyProductFlowSettingRequest request, string? userId)
        {
            return _memberService.GetActiveMembersByUserIds(new List<string>() { userId }, request.CompId).Count > 0;
        }

        private bool BeValidGroup(CreateOrUpdateApplyProductFlowSettingRequest request, string? groupId)
        {
            return _groupService.GetActiveGroupsByGroupIdList(new List<string>() { groupId }, request.CompId).Count > 0;
        }

        private  bool BeValidUserOrNull(CreateOrUpdateApplyProductFlowSettingRequest request, string? userId)
        {
            if(userId==null) { return true; };
            return _memberService.GetActiveMembersByUserIds(new List<string>() { userId },request.CompId).Count > 0;
        }

        private bool BeValidGroupOrNull(CreateOrUpdateApplyProductFlowSettingRequest request, string? groupId)
        {
            if (groupId == null) { return true; };
            return _groupService.GetActiveGroupsByGroupIdList(new List<string>() { groupId }, request.CompId).Count > 0;
        }
    }
}
