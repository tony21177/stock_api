using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class AnswerFlowValidator : AbstractValidator<AnswerFlowRequest>
    {
        public AnswerFlowValidator()
        {
            RuleFor(request => request.Answer)
               .Must(answer => CommonConstants.AnswerPurchaseFlow.GetAllValues().Contains(answer))
                   .WithMessage($"answer必須為{string.Join(",", CommonConstants.AnswerPurchaseFlow.GetAllValues())}");
        }
    }
}
