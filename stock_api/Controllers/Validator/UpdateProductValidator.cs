using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
    {
        private readonly SupplierService _supplierService;
        private readonly GroupService _groupService;

        public UpdateProductValidator(SupplierService supplierService, GroupService groupService)
        {
            _supplierService = supplierService;
            _groupService = groupService;

            RuleFor(x => x.GroupIds)
                    .Must((request, groupIds, context) => BeValidGroupList(groupIds, context))
                    .WithMessage("以下 groupId 為無效的 group: {InvalidGroupIds}");
            RuleFor(request => request.QcType)
                 .Cascade(CascadeMode.Stop) // Stop on first failure
                 .Must(type => CommonConstants.QcTypeConstants.GetAllValues().Contains(type))
                 .When(request => request.QcType != null )
                     .WithMessage($"qcType必須為{string.Join(",", CommonConstants.QcTypeConstants.GetAllValues())}");


        }


        private bool BeValidGroupList(List<string> groupIds, ValidationContext<UpdateProductRequest> context)
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
    }
}
