using FluentValidation;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ListAdjustItemsValidator : AbstractValidator<ListAdjustItemsRequest>
    {
     
        private readonly SupplierService _supplierService;

        public ListAdjustItemsValidator(SupplierService supplierService)
        {
            _supplierService = supplierService;

            RuleFor(x => x.StartDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.EndDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            
            RuleFor(request => request.Type)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(type => CommonConstants.AdjustType.GetAllValues().Contains(type)).When(request => !string.IsNullOrEmpty(request.Type)) // Only validate when Type is not empty
                    .WithMessage($"type必須為{string.Join(",", CommonConstants.AdjustType.GetAllValues())}");
            RuleFor(request => request.CurrentStatus)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(status => CommonConstants.AdjustStatus.GetAllValues().Contains(status)).When(request => !string.IsNullOrEmpty(request.CurrentStatus)) // Only validate when Type is not empty
                    .WithMessage($"currentStatus必須為{string.Join(",", CommonConstants.AdjustStatus.GetAllValues())}");

        }


        private static bool BeValidDate(string? date, ValidationContext<ListAdjustItemsRequest> context)
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
