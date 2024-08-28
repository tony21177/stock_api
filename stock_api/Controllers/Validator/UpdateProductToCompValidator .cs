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


            
            RuleFor(x => x.ToCompId).NotEmpty()
                    .WithMessage("compIds不可為空");
            RuleFor(x => x.ToCompId)
                    .Must((request,toCompId, context) => BeValidToCompId(toCompId, context))
                    .WithMessage("該組織不存在或已停用");
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

        private bool BeValidToCompId(string toCompId, ValidationContext<UpdateProductToCompRequest> context)
        {

            var toComp = _companyService.GetCompanyWithUnitByCompanyId(toCompId);
           

            if (toComp==null||toComp.IsActive==false)
            {
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
