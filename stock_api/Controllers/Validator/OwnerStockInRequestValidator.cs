using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    

    public class OwnerStockInRequestValidator : AbstractValidator<OwnerStockInRequest>
    {
        public OwnerStockInRequestValidator()
        {
            RuleFor(x => x.ExpirationDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");

            RuleFor(x => x.Quantity).GreaterThan(0)
                .WithMessage("必須大於0");

            
        }
        private static bool BeValidDate(string? date, ValidationContext<OwnerStockInRequest> context)
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
