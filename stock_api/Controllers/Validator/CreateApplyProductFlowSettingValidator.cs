using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;
using MaiBackend.PublicApi.Consts;
using Org.BouncyCastle.Asn1.Ocsp;
using Google.Protobuf.WellKnownTypes;

namespace stock_api.Controllers.Validator
{

    public class CreateApplyProductFlowSettingValidator : AbstractValidator<CreateApplyProductFlowSettingRequest>
    {
        private readonly ApplyProductFlowSettingService _applyProductFlowSettingService;
        private readonly MemberService _memberService;
        private readonly GroupService _groupService;

        public CreateApplyProductFlowSettingValidator(ApplyProductFlowSettingService applyProductFlowSettingService, MemberService memberService, GroupService groupService)
        {
            _applyProductFlowSettingService = applyProductFlowSettingService;
            _memberService = memberService;
            _groupService = groupService;
            RuleForEach(request => request.CreateApplyProductFlowSettingList).SetValidator(new ApplyProductFlowSettingValidator(ActionTypeEnum.Create,applyProductFlowSettingService,memberService,groupService));
        }
    }



    public class ApplyProductFlowSettingValidator : AbstractValidator<ApplyProductFlowSettingRequest>
    {
        private readonly ApplyProductFlowSettingService _applyProductFlowSettingService;
        private readonly MemberService _memberService;
        private readonly GroupService _groupService;

        public ApplyProductFlowSettingValidator(ActionTypeEnum action, ApplyProductFlowSettingService applyProductFlowSettingService,
            MemberService memberService,  GroupService groupService)
        {
            _applyProductFlowSettingService = applyProductFlowSettingService;
            _memberService = memberService;
            _groupService = groupService;
            ClassLevelCascadeMode = CascadeMode.Stop;

            if (action == ActionTypeEnum.Create)
            {
                RuleFor(x => x.FlowName).NotEmpty().WithMessage("flowName為必須");
                RuleFor(x => x.Sequence).NotEmpty().WithMessage("sequence為必須");
                RuleFor(x => x.Sequence).Must((request, sequence, context) => SequenceUnique(request, sequence.Value)).WithMessage("sequence已存在");
                RuleFor(x => x).Custom((x, context) =>
                {
                    if (string.IsNullOrEmpty(x.ReviewUserId) || string.IsNullOrEmpty(x.ReviewGroupId))
                    {
                        context.AddFailure("", "ReviewUserId 和 ReviewGroupId 都需要");
                    }
                });
                RuleFor(x => x.ReviewUserId).Must((request, userId, context) => BeValidUser(request.CompId, userId))
                    .When(x=>x.ReviewUserId!=null).WithMessage("reviewUserId不存在");
                RuleFor(x => x.ReviewGroupId).Must((request, groupId, context) => BeValidGroup(request.CompId, groupId))
                    .When(x => x.ReviewGroupId != null).WithMessage("reviewGroupId不存在");
            }
            if (action == ActionTypeEnum.Update)
            {
                RuleFor(x => x.SettingId).NotEmpty().WithMessage("settingId為必須");
                RuleFor(x => x).Custom((x, context) =>
                {
                    if (!string.IsNullOrEmpty(x.ReviewUserId) || !string.IsNullOrEmpty(x.ReviewGroupId))
                    {
                        if (string.IsNullOrEmpty(x.ReviewUserId)|| string.IsNullOrEmpty(x.ReviewGroupId))
                        {
                            context.AddFailure("", "ReviewUserId 和 ReviewGroupId 都需要");

                        }
                    }
                });
                RuleFor(x => x.Sequence).Must((request, sequence, context) => SequenceUnique(request, sequence.Value))
                    .When(x=>x.Sequence!=null)
                    .WithMessage("sequence已存在");
                RuleFor(x => x.ReviewUserId).Must((request, userId, context) => BeValidUser(request.CompId, userId))
                    .When(x => x.ReviewUserId != null).WithMessage("reviewUserId不存在");
                RuleFor(x => x.ReviewGroupId).Must((request, groupId, context) => BeValidGroup(request.CompId, groupId))
                    .When(x => x.ReviewGroupId != null).WithMessage("reviewGroupId不存在");
            }
            _groupService = groupService;
        }

        private bool SequenceUnique(ApplyProductFlowSettingRequest request,int sequence)
        {
            return !_applyProductFlowSettingService.IsSequenceExist(sequence, request.CompId);
        }
        
        private bool BeValidUser(string compId, string? userId)
        {
            return _memberService.GetActiveMembersByUserIds(new List<string>() { userId }, compId).Count > 0;
        }

        private bool BeValidGroup(string compId, string? groupId)
        {
            return _groupService.GetActiveGroupsByGroupIdList(new List<string>() { groupId }, compId).Count > 0;
        }

    }
}
