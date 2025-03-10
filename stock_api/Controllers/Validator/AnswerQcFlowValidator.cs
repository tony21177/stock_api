using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class AnswerQcFlowValidator : AbstractValidator<AnswerQcFlowRequest>
    {
        public AnswerQcFlowValidator()
        {
            RuleFor(request => request.Answer)
               .Must(answer => CommonConstants.AnswerQcFlow.GetAllValues().Contains(answer))
                   .WithMessage($"answer必須為{string.Join(",", CommonConstants.AnswerQcFlow.GetAllValues())}");
        }
    }
}
