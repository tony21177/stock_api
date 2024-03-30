using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class UpdateSupplierValidator : AbstractValidator<UpdateSupplierRequest>
    {
        private readonly CompanyService _companyService;
        public UpdateSupplierValidator(CompanyService companyService)
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("id為必須");
            _companyService = companyService;
        }


        private bool BeValidCompany(string? compId, ValidationContext<CreateSupplierRequest> context)
        {
            if (compId == null) return true;
            var company = _companyService.GetCompanyByCompId(compId);

            if (company == null)
            {
                return false;
            }
            return true;
        }
    }
}
