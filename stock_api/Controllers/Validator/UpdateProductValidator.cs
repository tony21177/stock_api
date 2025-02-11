using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
    {
        private readonly SupplierService _supplierService;
        private readonly InstrumentService _instrumentService;
        private readonly GroupService _groupService;

        public UpdateProductValidator(SupplierService supplierService, GroupService groupService, InstrumentService instrumentService)
        {
            _supplierService = supplierService;
            _groupService = groupService;
            _instrumentService = instrumentService;

            RuleFor(x => x.GroupIds)
                    .Must((request, groupIds, context) => BeValidGroupList(groupIds, context))
                    .WithMessage("以下 groupId 為無效的 group: {InvalidGroupIds}");
            RuleFor(x => x.InstrumentIds)
                    .Must((request, instrumentIds, context) => BeValidInstrumentList(instrumentIds, context))
                    .WithMessage("以下 instrumentId 為無效的 儀器: {InvalidInstrumentIds}");
            RuleFor(request => request.QcType)
                 .Cascade(CascadeMode.Stop) // Stop on first failure
                 .Must(type => CommonConstants.QcTypeConstants.GetAllValues().Contains(type))
                 .When(request => request.QcType != null)
                     .WithMessage($"qcType必須為{string.Join(",", CommonConstants.QcTypeConstants.GetAllValues())}");
            _instrumentService = instrumentService;
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

        private bool BeValidInstrumentList(List<int> instrumentIds, ValidationContext<UpdateProductRequest> context)
        {
            if (instrumentIds == null || instrumentIds.Count == 0)
            {
                return true; 
            }

            var instrumentList = _instrumentService.GetByIdList(instrumentIds);

            var notExistInstrumentIds = instrumentIds.Except(instrumentList.Select(m => m.InstrumentId)).ToList();

            if (notExistInstrumentIds.Any())
            {
                var errorMessage = $"{string.Join(",", notExistInstrumentIds)}";
                context.MessageFormatter.AppendArgument("InvalidInstrumentIds", errorMessage);
                return false;
            }
            return true;
        }
    }
}
