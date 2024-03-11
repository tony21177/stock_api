using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;
using MaiBackend.PublicApi.Consts;

namespace stock_api.Controllers.Validator
{
    public class CreateOrUpdateSheetSettingGroupRequestValidator : AbstractValidator<CreateOrUpdateSheetSettingGroupRequest>
    {
        private readonly HandoverService _handoverService;
        public CreateOrUpdateSheetSettingGroupRequestValidator(ActionTypeEnum action, HandoverService handoverService)
        {
            _handoverService = handoverService;

            if (action == ActionTypeEnum.Create)
            {
                RuleFor(x => x.MainSheetId).NotEmpty().WithMessage("mainSheetId為必須");
                RuleFor(x => x.GroupTitle).NotEmpty().WithMessage("groupTitle為必須");
                RuleFor(x => x.GroupDescription).NotNull().WithMessage("groupDescription");
                RuleFor(x => x.GroupRank).NotEmpty().WithMessage("groupRank為必須");
                RuleFor(x => x.IsActive).NotEmpty().WithMessage("isActive為必須");
                RuleFor(x => x.MainSheetId).Must(ExistSheetMain).WithMessage("此SheetMain不存在");

            }
            if (action == ActionTypeEnum.Update)
            {
                RuleFor(x => x.SheetGroupId).NotEmpty().WithMessage("sheetGroupId為必須");
            }

            
        }

        private bool ExistSheetMain(int? mainSheetId)
        {
            if (mainSheetId == null) return false;
            var existNaub = _handoverService.GetSheetMainByMainSheetId(mainSheetId.Value);
            return existNaub != null;
        }

    }
}
