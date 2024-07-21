using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class AnswerApplyNewProductFlowValidator : AbstractValidator<AnswerFlowRequest>
    {
        public AnswerApplyNewProductFlowValidator()
        {
            RuleFor(request => request.Answer)
               .Must(answer => CommonConstants.AnswerApplyNewProductFlow.GetAllValues().Contains(answer))
                   .WithMessage($"answer必須為{string.Join(",", CommonConstants.AnswerApplyNewProductFlow.GetAllValues())}");
        }
    }
}
