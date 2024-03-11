using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class UpdateAnnouncementValidator : AbstractValidator<UpdateAnnouncementRequest>
    {
        private readonly MemberService _memberService;

        public UpdateAnnouncementValidator(MemberService memberService)
        {
            _memberService = memberService;

            RuleFor(x => x.ReaderUserIdList)
                .Must((request, userIds, context) => BeValidUserList(userIds, context))
                .WithMessage("以下 userId 為無效的 user: {InvalidUserIds}");
        }

        private bool BeValidUserList(List<string> userIds, ValidationContext<UpdateAnnouncementRequest> context)
        {
            if (userIds == null || userIds.Count == 0)
            {
                return true; // 允許空的 userIds
            }

            var activeMemberList = _memberService.GetActiveMembersByUserIds(userIds);
            var notExistUserIds = userIds.Except(activeMemberList.Select(m => m.UserId)).ToList();

            if (notExistUserIds.Any())
            {
                var errorMessage = $"{string.Join(",", notExistUserIds)}";
                context.MessageFormatter.AppendArgument("InvalidUserIds", errorMessage);
                return false;
            }
            return true;
        }
    }
}
