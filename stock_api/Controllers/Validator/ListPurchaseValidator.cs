using FluentValidation;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ListPurchaseValidator : AbstractValidator<ListPurchaseRequest>
    {
        private readonly WarehouseProductService _warehouseProductService;
        private readonly GroupService _groupService;

        public ListPurchaseValidator(WarehouseProductService warehouseProductService, GroupService groupService)
        {
            _warehouseProductService = warehouseProductService;
            _groupService = groupService;

            RuleFor(x => x.StartDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.EndDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.GroupId)
                .Must((request, groupId, context) => BeValidGroup(groupId, context))
                .WithMessage("該群組不存在");
            RuleFor(request => request.Type)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(type => CommonConstants.PurchaseType.GetAllValues().Contains(type)).When(request => !string.IsNullOrEmpty(request.Type)) // Only validate when Type is not empty
                    .WithMessage($"type必須為{string.Join(",", CommonConstants.PurchaseType.GetAllValues())}");
            RuleFor(request => request.CurrentStatus)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(status => CommonConstants.PurchaseApplyStatus.GetAllValues().Contains(status)).When(request => !string.IsNullOrEmpty(request.CurrentStatus)) // Only validate when Type is not empty
                    .WithMessage($"currentStatus必須為{string.Join(",", CommonConstants.PurchaseApplyStatus.GetAllValues())}");
            RuleFor(request => request.ReceiveStatus)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(status => CommonConstants.PurchaseReceiveStatus.GetAllValues().Contains(status)).When(request => !string.IsNullOrEmpty(request.ReceiveStatus)) 
                    .WithMessage($"receiveStatus必須為{string.Join(",", CommonConstants.PurchaseReceiveStatus.GetAllValues())}");

        }


        private static bool BeValidDate(string? date, ValidationContext<ListPurchaseRequest> context)
        {
            if (date == null)
            {
                return true;
            }

            if (DateTimeHelper.ParseDateString(date) != null) return true;
            return false;
        }

        private bool BeValidGroup(string? groupId, ValidationContext<ListPurchaseRequest> context)
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
