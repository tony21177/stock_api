using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;
using MaiBackend.PublicApi.Consts;
using System.Text.RegularExpressions;

namespace stock_api.Controllers.Validator
{
    public class CreateOrUpdateSheetSettingRowRequestValidator : AbstractValidator<CreateOrUpdateSheetSettingRowRequest>
    {
        private readonly HandoverService _handoverService;
        public CreateOrUpdateSheetSettingRowRequestValidator(ActionTypeEnum action, HandoverService handoverService)
        {
            _handoverService = handoverService;

            if (action == ActionTypeEnum.Create)
            {
                RuleFor(x => x.MainSheetId).NotEmpty().WithMessage("mainSheetId為必須");
                RuleFor(x => x.SheetGroupId).NotEmpty().WithMessage("sheetGroupId為必須");
                RuleFor(x => x.WeekDays).Must(ValidWeekDays).WithMessage("weekDays為以,分隔的1-7數字");
                RuleFor(x => x.IsActive).NotNull().WithMessage("isActive為必須");

            }
            if (action == ActionTypeEnum.Update)
            {
                RuleFor(x => x.SheetRowId).NotEmpty().WithMessage("sheetRowId為必須");
            }
        }

        private bool ValidWeekDays(string? weekDays)
        {
            if (string.IsNullOrEmpty(weekDays)) return false;

            // 定義正則表達式，匹配1到7之間的數字，並以逗號分隔
            string pattern = @"^([1-7],)*[1-7]$";

            // 使用正則表達式進行匹配
            return Regex.IsMatch(weekDays, pattern);
        }

    }
}
