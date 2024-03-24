using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class UpdateCompanyValidator : AbstractValidator<UpdateCompanyRequest>
    {
        public UpdateCompanyValidator()
        {
            RuleFor(x => x.CompId).NotEmpty().WithMessage("compId為必須");
        }
    }
}
