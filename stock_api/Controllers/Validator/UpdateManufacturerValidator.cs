using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class UpdateManufacturerValidator : AbstractValidator<UpdateManufacturerRequest>
    {
        public UpdateManufacturerValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("id為必須");
        }
    }
}
