using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class CreateQcValidator : AbstractValidator<CreateQcRequest>
    {
        public CreateQcValidator()
        {
            RuleFor(request => request.QcType)
                 .Cascade(CascadeMode.Stop) // Stop on first failure
                 .Must(type => CommonConstants.QcTypeConstants.GetAllValues().Contains(type))
                     .WithMessage($"qcType必須為{string.Join(",", CommonConstants.QcTypeConstants.GetAllValues())}");
            RuleFor(x => x).Custom((x, context) =>
            {
                if (string.IsNullOrEmpty(x.LotNumber) && string.IsNullOrEmpty(x.LotNumberBatch))
                {
                    context.AddFailure("lotNumberOrBatch", "lotNumber批號或lotNumberBatch批次至少需一個");
                }
            });

            RuleFor(request => request.FinalResult)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(result => CommonConstants.QcFinalResult.GetAllValues().Contains(result)).When(request => !string.IsNullOrEmpty(request.FinalResult)) 
                    .WithMessage($"finalResult必須為{string.Join(",", CommonConstants.QcFinalResult.GetAllValues())}");
            RuleFor(request => request.NewLotNumberTestResult)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(result => CommonConstants.NewLotNumberTestResult.GetAllValues().Contains(result)).When(request => !string.IsNullOrEmpty(request.NewLotNumberTestResult)) 
                    .WithMessage($"newLotNumberTestResult必須為{string.Join(",", CommonConstants.NewLotNumberTestResult.GetAllValues())}");
            RuleFor(request => request.PreTestResult)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(result => CommonConstants.PreTestResult.GetAllValues().Contains(result)).When(request => !string.IsNullOrEmpty(request.PreTestResult)) 
                    .WithMessage($"preTestResult必須為{string.Join(",", CommonConstants.PreTestResult.GetAllValues())}");
            RuleFor(request => request.ReagentManual)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(manual => CommonConstants.ReagentManual.GetAllValues().Contains(manual)).When(request => !string.IsNullOrEmpty(request.ReagentManual))
                .WithMessage($"reagentManual必須為{string.Join(",", CommonConstants.ReagentManual.GetAllValues())}");
            RuleFor(request => request.IsReturnStock)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(isReturnStock => CommonConstants.IsReturnStock.GetAllValues().Contains(isReturnStock)).When(request => !string.IsNullOrEmpty(request.IsReturnStock))
                .WithMessage($"isReturnStock必須為{string.Join(",", CommonConstants.IsReturnStock.GetAllValues())}");
        }
    }
}
