using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;
using MaiBackend.PublicApi.Consts;
using System.Runtime.Serialization;

namespace stock_api.Controllers.Validator
{
    public class CreateOrUpdateMemberValidator : AbstractValidator<CreateOrUpdateMemberRequest>
    {
        private readonly AuthLayerService _authLayerService;
        private readonly MemberService _memberService;
        private readonly GroupService _groupService;

        public CreateOrUpdateMemberValidator(ActionTypeEnum action, AuthLayerService authLayerService, MemberService memberService, GroupService groupService)
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
                RuleFor(x=>x.Email).NotEmpty().WithMessage("email為必須")
                    .EmailAddress().WithMessage("無效的email格式");
                RuleFor(x => x.Agents)
                    .Must((request, agents, context) => BeValidUserList(agents, context))
                    .WithMessage("以下 agents 為無效的 user: {InvalidAgents}");
            }
            if (action == ActionTypeEnum.Update)
            {
                RuleFor(x => x.UserId).NotEmpty().WithMessage("userId為必須");
                //RuleFor(x => x.Email).EmailAddress().WithMessage("無效的email格式").When(email=>email!=null);
                RuleFor(x => x.Agents)
                   .Must((request, agents, context) => BeValidUserList(agents, context))
                   .WithMessage("以下 agents 為無效的 user: {InvalidAgents}");
            }
            _authLayerService = authLayerService;
            _memberService = memberService;
            _groupService = groupService;


            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x).Must(ExistAuthValue).When(x => x.AuthValue != null).WithMessage("此階層不存在");
            RuleFor(x => x.GroupIds)
                .Must((request, groupIds, context) => BeValidGroupList(groupIds, context))
                .When(x=>x.GroupIds!=null)
                .WithMessage("以下 groupId 為無效的 group: {InvalidGroupIds}");
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

        private bool BeValidGroupList(List<string> groupIds, ValidationContext<CreateOrUpdateMemberRequest> context)
        {
            if (groupIds == null || groupIds.Count == 0)
            {
                return true; // 允許空的 groupIds
            }

            var groupList = _groupService.GetGroupsByIdList(groupIds);
            var activeGroupList = groupList.Where(g => g.IsActive ==true).ToList(); 

            var notExistGroupIds = groupIds.Except(activeGroupList.Select(m => m.GroupId)).ToList();

            if (notExistGroupIds.Any())
            {
                var errorMessage = $"{string.Join(",", notExistGroupIds)}";
                context.MessageFormatter.AppendArgument("InvalidGroupIds", errorMessage);
                return false;
            }
            return true;
        }

        private bool BeValidUserList(List<string> agents, ValidationContext<CreateOrUpdateMemberRequest> context)
        {
            if (agents == null || agents.Count == 0)
            {
                return true; 
            }
            var members = _memberService.GetMembersByUserIdList(agents);
            var activeMembers = members.Where(m=>m.IsActive==true).ToList();

            var notExistAgentIds = agents.Except(activeMembers.Select(m => m.UserId)).ToList();

            if (notExistAgentIds.Any())
            {
                var errorMessage = $"{string.Join(",", notExistAgentIds)}";
                context.MessageFormatter.AppendArgument("InvalidAgents", errorMessage);
                return false;
            }
            return true;
        }
    }
}
