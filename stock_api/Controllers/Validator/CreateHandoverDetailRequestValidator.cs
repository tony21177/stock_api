﻿using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;
using MaiBackend.PublicApi.Consts;

namespace stock_api.Controllers.Validator
{
    public class CreateHandoverDetailRequestValidator : AbstractValidator<CreateHandoverDetailRequest>
    {
        private readonly MemberService _memberService;

        public CreateHandoverDetailRequestValidator(ActionTypeEnum action, MemberService memberService)
        {

            _memberService = memberService;
            if (action == ActionTypeEnum.Create)
            {
                RuleFor(x => x.RowDetails).NotEmpty().WithMessage("rowDetails為必須");
                RuleFor(x => x.Title).NotEmpty().WithMessage("titile為必須");
            }

            RuleFor(x => x.ReaderUserIds)
                .Must((request, userIds, context) => BeValidUserList(userIds, context))
                .WithMessage("以下 userId 為無效的 user: {InvalidUserIds}");

            ClassLevelCascadeMode = CascadeMode.Stop;
        }

        private bool BeValidUserList(List<string> userIds, ValidationContext<CreateHandoverDetailRequest> context)
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
