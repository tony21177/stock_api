using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class ListSupplierTraceLogValidator : AbstractValidator<ListSupplierTraceLogRequest>
    {
        public ListSupplierTraceLogValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(request => request.SourceType)
               .Must(type => CommonConstants.SourceType.GetAllValues().Contains(type))
               .When(request => request.SourceType != null)
                   .WithMessage($"sourceType必須為{string.Join(",", CommonConstants.SourceType.GetAllValues())}");

            RuleFor(request => request.AbnormalType)
               .Must(abnormalType => CommonConstants.AbnormalType.GetAllValues().Contains(abnormalType))
               .When(request => request.AbnormalType != null)
                   .WithMessage($"abnormalType必須為{string.Join(",", CommonConstants.AbnormalType.GetAllValues())}");
        }
    }
}
