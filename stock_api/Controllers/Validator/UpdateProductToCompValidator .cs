using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class UpdateProductToCompValidator : AbstractValidator<UpdateProductToCompRequest>
    {
        private readonly CompanyService _companyService;
        private readonly WarehouseProductService _warehouseProductService;

        public UpdateProductToCompValidator( CompanyService companyService,WarehouseProductService warehouseProductService)
        {
            
            _companyService = companyService;
            _warehouseProductService = warehouseProductService;


            
            RuleFor(x => x.ToCompIds).NotEmpty()
                    .WithMessage("toCompIds不可為空");
            RuleFor(x => x.ToCompIds)
                    .Must((request,toCompIds, context) => BeValidToCompIds(toCompIds, context))
                    .WithMessage("以下 compId 為無效的comp: {InvalidComps}");
            RuleFor(x => x.FromCompId)
                    .Must((request, fromCompId, context) => BeValidFromCompId(fromCompId, context))
                    .WithMessage("fromCompId不存在或是非得標廠商");
            RuleFor(x => x.ProductCode)
                    .Must((request, productCode, context) => BeValidProduct(productCode,request, context))
                    .WithMessage("該product不存在");

        }

        private bool BeValidProduct(string productCode, UpdateProductToCompRequest request, ValidationContext<UpdateProductToCompRequest> context)
        {

            var product = _warehouseProductService.GetProductByProductCodeAndCompId(productCode, request.FromCompId);


            if (product == null )
            {
                return false;
            }
            return true;
        }

        private bool BeValidToCompIds(List<string> toCompIds, ValidationContext<UpdateProductToCompRequest> context)
        {

            var toComps = _companyService.GetCompanyWithUnitByCompanyIds(toCompIds);


            var notExistCompIds = toComps.Select(c=>c.CompId).Except(toCompIds).ToList();

            if (notExistCompIds.Any())
            {
                var errorMessage = $"{string.Join(",", notExistCompIds)}";
                context.MessageFormatter.AppendArgument("InvalidComps", errorMessage);
                return false;
            }

            
            return true;
        }

        private bool BeValidFromCompId(string fromCompId, ValidationContext<UpdateProductToCompRequest> context)
        {

            var fromComp = _companyService.GetCompanyWithUnitByCompanyId(fromCompId);


            if (fromComp == null || fromComp.Type!=CommonConstants.CompanyType.OWNER)
            {
                return false;
            }
            return true;
        }
    }
}
