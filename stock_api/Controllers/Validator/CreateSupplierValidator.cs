using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class CreateSupplierValidator : AbstractValidator<CreateSupplierRequest>
    {
        private readonly CompanyService _companyService;

        public CreateSupplierValidator(CompanyService companyService)
        {
            _companyService = companyService;
            RuleFor(x => x.Name).NotEmpty().WithMessage("name為必須");
            RuleFor(x => x.Code).NotEmpty().WithMessage("code為必須");
            RuleFor(x => x.IsActive).NotEmpty().WithMessage("isActive為必須");
            RuleFor(x => x.CompId)
                .Must((request, compId, context) => BeValidCompany(compId, context))
                .WithMessage("該組織不存在");
        }

        private bool BeValidCompany(string compId, ValidationContext<CreateSupplierRequest> context)
        {
            var company = _companyService.GetCompanyByCompId(compId); 

            if (company==null)
            {
                return false;
            }
            return true;
        }
    }
}
