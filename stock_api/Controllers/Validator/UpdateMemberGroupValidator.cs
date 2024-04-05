using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class UpdateMemberGroupValidator : AbstractValidator<UpdateMemberGroupRequest>
    {
        private readonly GroupService _groupService;
        public UpdateMemberGroupValidator(GroupService groupService) {
            RuleFor(x => x.GroupIds)
                    .Must((request, groupIds, context) => BeValidGroupList(groupIds, context))
                    .WithMessage("以下 groupId 為無效的 group: {InvalidGroupIds}");
            _groupService = groupService;
        }

        private bool BeValidGroupList(List<string> groupIds, ValidationContext<UpdateMemberGroupRequest> context)
        {
            if (groupIds == null || groupIds.Count == 0)
            {
                return true; // 允許空的 groupIds
            }

            var groupList = _groupService.GetGroupsByIdList(groupIds);
            var activeGroupList = groupList.Where(g => g.IsActive == true).ToList();

            var notExistGroupIds = groupIds.Except(activeGroupList.Select(m => m.GroupId)).ToList();

            if (notExistGroupIds.Any())
            {
                var errorMessage = $"{string.Join(",", notExistGroupIds)}";
                context.MessageFormatter.AppendArgument("InvalidGroupIds", errorMessage);
                return false;
            }
            return true;
        }
    }
}
