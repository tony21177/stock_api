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

            RuleFor(item => item.PackagingStatus)
                .Must(status => CommonConstants.PackagingStatus.GetAllValues().Contains(status))
                .When(item => item.PackagingStatus != null) 
                .WithMessage($"packagingStatus必須為{string.Join(",", CommonConstants.PackagingStatus.GetAllValues())}");

            RuleFor(item => item.DeliverFunction)
                .Must(function => CommonConstants.DeliverFunctionType.GetAllValues().Contains(function))
                .When(item => item.DeliverFunction != null) 
                .WithMessage($"deliverFunction必須為{string.Join(",", CommonConstants.DeliverFunctionType.GetAllValues())}");

            RuleFor(item => item.SavingFunction)
                .Must(status => CommonConstants.SavingFunctionType.GetAllValues().Contains(status))
                .When(item => item.SavingFunction != null) 
                .WithMessage($"savingFunction必須為{string.Join(",", CommonConstants.SavingFunctionType.GetAllValues())}");
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
