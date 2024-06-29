using FluentValidation;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class AdminUpdateProductValidator : AbstractValidator<AdminUpdateProductRequest>
    {
        private readonly SupplierService _supplierService;
        private readonly GroupService _groupService;
        private readonly ManufacturerService _manufacturerService;

        public AdminUpdateProductValidator(SupplierService supplierService, GroupService groupService, ManufacturerService manufacturerService)
        {
            _supplierService = supplierService;
            _groupService = groupService;
            _manufacturerService = manufacturerService;

            RuleFor(x => x.GroupIds)
                    .Must((request, groupIds, context) => BeValidGroupList(groupIds, context))
                    .WithMessage("以下 groupId 為無效的 group: {InvalidGroupIds}");
            RuleFor(x => x.DefaultSupplierId)
                .Must((request, supplierId, context) => BeValidSupplier(supplierId, context))
                    .WithMessage("無效的供應商");
            RuleFor(x => x.ManufacturerId)
                .Must((request, manufacturerId, context) => BeValidManufacturer(manufacturerId, context))
                    .WithMessage("無效的製造商");
            RuleFor(x=>x.LastAbleDate)
                .Must((request, lastAbleDate, context) => BeValidDate(lastAbleDate, context))
                    .WithMessage("無效的日期");
            RuleFor(x => x.LastOutStockDate)
                .Must((request, lastOutStockDate, context) => BeValidDate(lastOutStockDate, context))
                    .WithMessage("無效的日期");
            RuleFor(x => x.OriginalDeadline)
                .Must((request, originalDeadline, context) => BeValidDate(originalDeadline, context))
                    .WithMessage("無效的日期");
        }


        private bool BeValidGroupList(List<string> groupIds, ValidationContext<AdminUpdateProductRequest> context)
        {
            if (groupIds == null || groupIds.Count == 0)
            {
                return true; // 允許空的 groupIds
            }

            var groupList = _groupService.GetGroupsByIdList(groupIds);
            var activeGroupList = groupList.Where(g => g.IsActive == true).ToList();

            var notExistGroupIds = groupIds.Except(activeGroupList.Select(m => m.GroupId)).ToList();

            if (notExistGroupIds.Any())
            {
                var errorMessage = $"{string.Join(",", notExistGroupIds)}";
                context.MessageFormatter.AppendArgument("InvalidGroupIds", errorMessage);
                return false;
            }
            return true;
        }

        private bool BeValidSupplier(int? supplierId, ValidationContext<AdminUpdateProductRequest> context)
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

        private bool BeValidManufacturer(string? manufacturerId, ValidationContext<AdminUpdateProductRequest> context)
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

        private static bool BeValidDate(string? date, ValidationContext<AdminUpdateProductRequest> context)
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
