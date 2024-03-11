using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;
using MaiBackend.PublicApi.Consts;

namespace stock_api.Controllers.Validator
{
    public class CreateOrUpdateAnnouncementValidator : AbstractValidator<CreateAnnoucementRequest>
    {
        private readonly MemberService _memberService;

        public CreateOrUpdateAnnouncementValidator(ActionTypeEnum action, MemberService memberService)
        {
            _memberService = memberService;

            RuleFor(x => x.ReaderUserIdList)
                .Must((request, userIds, context) => BeValidUserList(userIds, context))
                .WithMessage("以下 userId 為無效的 user: {InvalidUserIds}");

            if (action == ActionTypeEnum.Create)
            {
                RuleFor(x => x.Title).NotEmpty().WithMessage("title 欄位為必填");
                RuleFor(x => x.Content).NotEmpty().WithMessage("content 欄位為必填");
            }
        }

        private bool BeValidUserList(List<string> userIds, ValidationContext<CreateAnnoucementRequest> context)
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
