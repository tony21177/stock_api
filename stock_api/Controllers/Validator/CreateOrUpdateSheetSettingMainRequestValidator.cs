using FluentValidation;
using stock_api.Controllers.Request;
using MaiBackend.PublicApi.Consts;

namespace stock_api.Controllers.Validator
{
    public class CreateOrUpdateSheetSettingMainRequestValidator : AbstractValidator<CreateOrUpdateSheetSettingMainRequest>
    {
        public CreateOrUpdateSheetSettingMainRequestValidator(ActionTypeEnum action)
        {
            if (action == ActionTypeEnum.Create)
            {
                RuleFor(x => x.Title).NotEmpty().WithMessage("title為必須");
                RuleFor(x => x.Description).NotEmpty().WithMessage("description為必須");
                RuleFor(x => x.IsActive).NotNull().WithMessage("isActive為必須");
                RuleFor(x => x.Version).NotEmpty().WithMessage("version為必須");
                RuleFor(x => x.SerialCode).NotEmpty().WithMessage("serialCode為必須");

            }
            if (action == ActionTypeEnum.Update)
            {
                RuleFor(x => x.SheetId).NotEmpty().WithMessage("sheetId為必須");
            }

        }
    }
}
