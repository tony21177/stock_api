using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class ListStockOutRecordsValidator : AbstractValidator<ListStockOutRecordsRequest>
    {
        public ListStockOutRecordsValidator()
        {
            RuleFor(request => request.Type)
                 .Must(type => CommonConstants.OutStockType.GetAllValues().Contains(type))
                 .When(request => request.Type != null)
                     .WithMessage($"type必須為{string.Join(",", CommonConstants.OutStockType.GetAllValues())}");
        }
    }
}
