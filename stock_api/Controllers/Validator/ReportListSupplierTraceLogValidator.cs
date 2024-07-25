using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ReportListSupplierTraceLogValidator : AbstractValidator<ReportListSupplierTraceLogRequest>
    {
        private readonly SupplierService _supplierService;

        public ReportListSupplierTraceLogValidator(SupplierService supplierService)
        {
            _supplierService = supplierService;
            ClassLevelCascadeMode = CascadeMode.Stop;


            RuleFor(x => x.SupplierId)
                .Must((request, supplierId, context) => BeValidSupplier(supplierId, context))
                    .WithMessage("無效的供應商");
            RuleFor(x => x.StartDate).Must((request, date, context) => BeValidDate(date, context))
                .When(x => x.StartDate != null)
                .WithMessage("無效格式日期");
            RuleFor(x => x.EndDate).Must((request, date, context) => BeValidDate(date, context))
                .When(x=>x.EndDate!=null)
                .WithMessage("無效格式日期");


        }
        private bool BeValidSupplier(int? supplierId, ValidationContext<ReportListSupplierTraceLogRequest> context)
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

        private static bool BeValidDate(string? date, ValidationContext<ReportListSupplierTraceLogRequest> context)
        {
            if (date == null)
            {
                return true;
            }

            if (DateTimeHelper.ParseDateString(date) != null) return true;
            return false;
        }
    }
}
