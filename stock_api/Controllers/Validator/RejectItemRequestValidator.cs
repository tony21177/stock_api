using FluentValidation;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class RejectItemRequestValidator : AbstractValidator<RejectItemRequest>
    {
        public RejectItemRequestValidator()
        {
            RuleFor(x => x.RejectDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.RejectQuantity).GreaterThan(0).When(x => x.RejectQuantity != null)
                .WithMessage("數量必須大於0");
        }

        private static bool BeValidDate(string? date, ValidationContext<RejectItemRequest> context)
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
