using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class UpdateBatchAcceptItemsRequestValidator : AbstractValidator<UpdateBatchAcceptItemsRequest>
    {
        public UpdateBatchAcceptItemsRequestValidator()
        {
            // 確保 UpdateAcceptItemList 不是 null 且其中的每個項目都被驗證
            RuleForEach(request => request.UpdateAcceptItemList).SetValidator(new UpdateAcceptItemValidator());
        }
    }

    public class UpdateAcceptItemValidator : AbstractValidator<UpdateAcceptItemRequest>
    {
        public UpdateAcceptItemValidator()
        {
            RuleFor(x => x.ExpirationDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");

            RuleFor(item => item.QcStatus)
                //.NotNull().WithMessage("QcStatus 是必填項目。")
                .Must(qcStatus => CommonConstants.QcStatus.GetAllValues().Contains(qcStatus))
                .When(item => item.QcStatus != null) // 當 QcStatus 不為 null 時
                .WithMessage($"qcStatus必須為{string.Join(",", CommonConstants.QcStatus.GetAllValues())}");
        }

        private static bool BeValidDate(string? date, ValidationContext<UpdateAcceptItemRequest> context)
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
