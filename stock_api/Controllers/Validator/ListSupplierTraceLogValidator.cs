using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ListSupplierTraceLogValidator : AbstractValidator<ListSupplierTraceLogRequest>
    {
        private readonly SupplierService _supplierService;

        public ListSupplierTraceLogValidator(SupplierService supplierService)
        {
            _supplierService = supplierService;
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(request => request.SourceType)
               .Must(type => CommonConstants.SourceType.GetAllValues().Contains(type))
               .When(request => request.SourceType != null)
                   .WithMessage($"sourceType必須為{string.Join(",", CommonConstants.SourceType.GetAllValues())}");

            RuleFor(request => request.AbnormalType)
               .Must(abnormalType => CommonConstants.AbnormalType.GetAllValues().Contains(abnormalType))
               .When(request => request.AbnormalType != null)
                   .WithMessage($"abnormalType必須為{string.Join(",", CommonConstants.AbnormalType.GetAllValues())}");

            RuleFor(x => x.SupplierId)
                .Must((request, supplierId, context) => BeValidSupplier(supplierId, context))
                    .WithMessage("無效的供應商");


        }
        private bool BeValidSupplier(int? supplierId, ValidationContext<ListSupplierTraceLogRequest> context)
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
    }
}
