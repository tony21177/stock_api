using FluentValidation;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ListRejectRecordsRequestValidator : AbstractValidator<ListRejectRecordsRequest>
    {
        

        public ListRejectRecordsRequestValidator()
        {
            
            RuleFor(x => x.StartDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.EndDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            

        }


        private static bool BeValidDate(string? date, ValidationContext<ListRejectRecordsRequest> context)
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
