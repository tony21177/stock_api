using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class UpdateInStockRequestValidator : AbstractValidator<UpdateInStockRequest>
    {
        public UpdateInStockRequestValidator()
        {
            RuleFor(x => x.ExpirationDate).Must((request, date, context) => BeValidDate(date, context))
                .When(item => item.ExpirationDate != null) // 
                .WithMessage("無效格式日期");
        }

        private static bool BeValidDate(string? date, ValidationContext<UpdateInStockRequest> context)
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
