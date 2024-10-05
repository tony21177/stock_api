using FluentValidation;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ListMyReviewPurchaseValidator : AbstractValidator<ListMyReviewPurchaseRequest>
    {
        private readonly WarehouseProductService _warehouseProductService;
        private readonly GroupService _groupService;

        public ListMyReviewPurchaseValidator(WarehouseProductService warehouseProductService, GroupService groupService)
        {
            _warehouseProductService = warehouseProductService;
            _groupService = groupService;

            RuleFor(x => x.StartDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.EndDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(request => request.Type)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(type => CommonConstants.PurchaseType.GetAllValues().Contains(type)).When(request => !string.IsNullOrEmpty(request.Type)) // Only validate when Type is not empty
                    .WithMessage($"type必須為{string.Join(",", CommonConstants.PurchaseType.GetAllValues())}");

        }


        private static bool BeValidDate(string? date, ValidationContext<ListMyReviewPurchaseRequest> context)
        {
            if (date == null)
            {
                return true;
            }

            if (DateTimeHelper.ParseDateString(date) != null) return true;
            return false;
        }

        private bool BeValidGroup(string? groupId, ValidationContext<ListMyReviewPurchaseRequest> context)
        {
            if (groupId == null)
            {
                return true; 
            }

            var group = _groupService.GetGroupByGroupId(groupId);
            if (group == null) return false;
            return true;
        }
    }
}
