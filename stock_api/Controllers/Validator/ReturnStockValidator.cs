using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ReturnStockValidator : AbstractValidator<ReturnRequest>
    {
        

        public ReturnStockValidator()
        {
            RuleFor(x => x.ReturnQuantity)
               .GreaterThan(0)
                   .WithMessage("必須大於0");
            
        }

        
    }
}
