using FluentValidation;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ListQcMainWithDetailValidator : AbstractValidator<ListMainWithDetailRequest>
    {
       

        public ListQcMainWithDetailValidator()
        {

            RuleFor(x => x.QcStartDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.QcEndDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
       
            RuleFor(request => request.QcType)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(type => CommonConstants.QcTypeConstants.GetAllValues().Contains(type)).When(request => !string.IsNullOrEmpty(request.QcType)) // Only validate when Type is not empty
                    .WithMessage($"qcYype必須為{string.Join(",", CommonConstants.QcTypeConstants.GetAllValues())}");
            

        }


        private static bool BeValidDate(string? date, ValidationContext<ListMainWithDetailRequest> context)
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
