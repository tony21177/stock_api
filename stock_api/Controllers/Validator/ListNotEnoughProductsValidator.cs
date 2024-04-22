using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ListNotEnoughProductsValidator : AbstractValidator<ListNotEnoughProductsRequest>
    {
        private readonly SupplierService _supplierService;
        private readonly GroupService _groupService;

        public ListNotEnoughProductsValidator(SupplierService supplierService, GroupService groupService)
        {
            _supplierService = supplierService;
            _groupService = groupService;

            RuleFor(x => x.GroupId)
               .Must((request, groupId, context) => BeValidGroup(groupId, context))
               .WithMessage("該群組不存在");

            RuleFor(x => x.SupplierId)
               .Must((request, supplierId, context) => BeValidSupplier(supplierId, context))
               .WithMessage("該供應商不存在");
        }


        private bool BeValidGroup(string? groupId, ValidationContext<ListNotEnoughProductsRequest> context)
        {
            if (groupId == null)
            {
                return true;
            }

            var group = _groupService.GetGroupByGroupId(groupId);
            if (group == null) return false;
            return true;
        }

        private bool BeValidSupplier(int? supplierId, ValidationContext<ListNotEnoughProductsRequest> context)
        {
            if (supplierId == null)
            {
                return true;
            }

            var supplier = _supplierService.GetSupplierById(supplierId.Value);
            if (supplier == null) return false;
            return true;
        }
    }
}
