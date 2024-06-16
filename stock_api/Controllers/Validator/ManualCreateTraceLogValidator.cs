using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ManualCreateTraceLogValidator : AbstractValidator<ManualCreateSupplierTraceLogRequest>
    {
        private readonly SupplierService _supplierService;
        private readonly PurchaseService _purchaseService;

        public ManualCreateTraceLogValidator(SupplierService supplierService)
        {
            _supplierService = supplierService;

            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.SupplierId)
                .Must((request, supplierId, context) => BeValidSupplier(supplierId, context))
                    .WithMessage("無效的供應商");

            RuleFor(x => x.AbnormalDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");

            RuleFor(request => request.AbnormalType)
               .Must(answer => CommonConstants.AbnormalType.GetAllValues().Contains(answer))
                   .WithMessage($"abnormalType必須為{string.Join(",", CommonConstants.AbnormalType.GetAllValues())}");

        }

        private bool BeValidSupplier(int? supplierId, ValidationContext<ManualCreateSupplierTraceLogRequest> context)
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


        private static bool BeValidDate(string? date, ValidationContext<ManualCreateSupplierTraceLogRequest> context)
        {
            if (date == null)
            {
                return true; 
            }

            if(DateTimeHelper.ParseDateString(date)!=null) return true;
            return false;
        }
    }


}
