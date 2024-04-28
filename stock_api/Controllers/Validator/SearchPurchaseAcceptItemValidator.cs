using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class SearchPurchaseAcceptItemValidator : AbstractValidator<SearchPurchaseAcceptItemRequest>
    {
        private readonly GroupService _groupService;

        public SearchPurchaseAcceptItemValidator(GroupService groupService)
        {
            _groupService = groupService;
            RuleFor(request => request.ReceiveStatus)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(receiveStatue => CommonConstants.PurchaseReceiveStatus.GetAllValues().Contains(receiveStatue)).When(request => !string.IsNullOrEmpty(request.ReceiveStatus)) // Only validate when Type is not empty
                    .WithMessage($"receiveStatue必須為{string.Join(",", CommonConstants.PurchaseReceiveStatus.GetAllValues())}");
            RuleFor(x => x.DemandDateStart).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.DemandDateEnd).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.ApplyDateStart).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.ApplyDateEnd).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.GroupId)
               .Must((request, groupId, context) => BeValidGroup(groupId, context))
               .WithMessage("該群組不存在");
            RuleFor(request => request.Type)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(type => CommonConstants.PurchaseType.GetAllValues().Contains(type)).When(request => !string.IsNullOrEmpty(request.Type)) // Only validate when Type is not empty
                    .WithMessage($"type必須為{string.Join(",", CommonConstants.PurchaseType.GetAllValues())}");
        }

        private bool BeValidGroup(string? groupId, ValidationContext<SearchPurchaseAcceptItemRequest> context)
        {
            if (groupId == null)
            {
                return true;
            }

            var group = _groupService.GetGroupByGroupId(groupId);
            if (group == null) return false;
            return true;
        }
        private static bool BeValidDate(string? date, ValidationContext<SearchPurchaseAcceptItemRequest> context)
        {
            if (date == null)
            {
                return true;
            }

            if (DateTimeHelper.ParseDateString(date) != null) return true;
            return false;
        }
    }
}
