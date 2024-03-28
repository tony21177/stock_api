using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class CreateCompanyValidator : AbstractValidator<CreateCompanyRequest>
    {
        public CreateCompanyValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("name為必須");
            RuleFor(x => x.IsActive).NotEmpty().WithMessage("isActive為必須");
        }
    }
}
