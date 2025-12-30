using FluentValidation;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class OwnerListPurchasesValidator : AbstractValidator<OwnerListPurchasesRequest>
    {
        public OwnerListPurchasesValidator()
        {
            RuleFor(x => x.ApplyDateStart).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.ApplyDateEnd).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
        }

        private static bool BeValidDate(string? date, ValidationContext<OwnerListPurchasesRequest> context)
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
