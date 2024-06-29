using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class AddNewProductValidator : AbstractValidator<AddNewProductRequest>
    {
        private readonly SupplierService _supplierService;
        private readonly ManufacturerService _manufacturerService;
        private readonly CompanyService _companyService;

        public AddNewProductValidator(SupplierService supplierService, ManufacturerService manufacturerService, CompanyService companyService)
        {
            _supplierService = supplierService;
            _manufacturerService = manufacturerService;
            _companyService = companyService;


            RuleFor(x => x.DefaultSupplierId)
               .Must((request, supplierId, context) => BeValidSupplier(supplierId, context))
                   .WithMessage("無效的供應商");
            RuleFor(x => x.ManufacturerId)
                .Must((request, manufacturerId, context) => BeValidManufacturer(manufacturerId, context))
                    .WithMessage("無效的製造商");
            RuleFor(x => x.OriginalDeadline)
                .Must((request, originalDeadline, context) => BeValidDate(originalDeadline, context))
                    .WithMessage("無效的日期");
            RuleFor(x => x.CompIds).NotEmpty()
                    .WithMessage("compIds不可為空");
            RuleFor(x => x.CompIds)
                    .Must((request, compIds, context) => BeValidCompList(compIds, context))
                    .WithMessage("以下 compId 為無效的 comp: {InvalidCompIds}");
            RuleFor(request => request.QcType)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(type => CommonConstants.QcTypeConstants.GetAllValues().Contains(type)).When(request => !string.IsNullOrEmpty(request.QcType)) // Only validate when Type is not empty
                    .WithMessage($"qcType{string.Join(",", CommonConstants.QcTypeConstants.GetAllValues())}");
        }

        private bool BeValidSupplier(int? supplierId, ValidationContext<AddNewProductRequest> context)
        {
            if (supplierId == null)
            {
                return true;
            }
            var supplier = _supplierService.GetSupplierById(supplierId.Value);
            if (supplier == null || supplier.IsActive == false)
            {
                return false;
            }
            return true;
        }

        private bool BeValidManufacturer(string? manufacturerId, ValidationContext<AddNewProductRequest> context)
        {
            if (manufacturerId == null)
            {
                return true;
            }
            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null || manufacturer.IsActive == false)
            {
                return false;
            }
            return true;
        }

        private bool BeValidDate(string? date, ValidationContext<AddNewProductRequest> context)
        {
            if (date == null)
            {
                return true;
            }
            if (DateTimeHelper.ParseDateString(date) != null) return true;
            return false;
        }

        private bool BeValidCompList(List<string> compIds, ValidationContext<AddNewProductRequest> context)
        {
            if (compIds == null || compIds.Count == 0)
            {
                return false; // 允許空的 groupIds
            }

            var compList = _companyService.GetCompanyByCompIds(compIds);
            var activeCompList = compList.Where(c => c.IsActive == true).ToList();

            var notExistCompIds = compIds.Except(activeCompList.Select(c => c.CompId)).ToList();

            if (notExistCompIds.Any())
            {
                var errorMessage = $"{string.Join(",", notExistCompIds)}";
                context.MessageFormatter.AppendArgument("InvalidCompIds", errorMessage);
                return false;
            }
            return true;
        }
    }
}
