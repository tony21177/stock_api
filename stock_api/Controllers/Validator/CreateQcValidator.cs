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
        }
    }
}
