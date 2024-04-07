using FluentValidation;
using stock_api.Service;

namespace stock_api.Controllers.Request
{
    public class SearchProductRequestValidator : AbstractValidator<WarehouseProductSearchRequest>
    {
        private readonly CompanyService _companyService;
        private readonly GroupService _groupService;

        public SearchProductRequestValidator(CompanyService companyService,GroupService groupService)
        {
            _companyService = companyService;
            _groupService = groupService;
            RuleFor(x => x.CompId)
                .Must((request, compId, context) => BeValidCompany(compId, context))
                .WithMessage("該組織不存在");
            RuleFor(x => x.GroupId)
                .Must((request, groupId, context) => BeValidGroup(groupId,context))
                .WithMessage("該群組不存在");
        }

        private bool BeValidCompany(string? compId, ValidationContext<WarehouseProductSearchRequest> context)
        {
            if (compId == null) return true;
            var company = _companyService.GetCompanyByCompId(compId);

            if (company == null)
            {
                return false;
            }
            return true;
        }

        private bool BeValidGroup(string? groupId, ValidationContext<WarehouseProductSearchRequest> context)
        {
            if (groupId == null) return true;
            var group = _groupService.GetGroupByGroupId(groupId);

            if (group == null)
            {
                return false;
            }
            return true;
        }
    }
}
