using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ResetAllAuthValidator : AbstractValidator<ResetAllAuthRequest>
    {
        private readonly CompanyService _companyService;

        public ResetAllAuthValidator(CompanyService companyService)
        {
            _companyService = companyService;

            RuleFor(x => x.CompId)
                .Must((request, compId, context) => BeValidComp(compId, context))
                .WithMessage("該組織不存在");
        }

        private bool BeValidComp(string compId, ValidationContext<ResetAllAuthRequest> context)
        {

            var comp = _companyService.GetCompanyByCompId(compId);
            if (comp == null) return false;
            if(comp.IsActive==false) return false;
            return true;
        }
    }
}
