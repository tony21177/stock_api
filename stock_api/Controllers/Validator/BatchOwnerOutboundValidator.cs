using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class BatchOwnerOutboundValidator : AbstractValidator<BatchOwnerOutboundRequest>
    {
        public BatchOwnerOutboundValidator()
        {
            RuleFor(request => request.Type)
                  .Must(type => CommonConstants.OutStockType.GetAllValues().Contains(type))
                      .WithMessage($"type必須為{string.Join(",", CommonConstants.OutStockType.GetAllValues())}");
        }

    }

    public class OwnerOutboundValidator : AbstractValidator<OwnerOutboundRequest>
    {
        public OwnerOutboundValidator() 
        {
            RuleFor(request => request.Type)
                  .Must(type => CommonConstants.OutStockType.GetAllValues().Contains(type))
                      .WithMessage($"type必須為{string.Join(",", CommonConstants.OutStockType.GetAllValues())}");
        }

    }
}
