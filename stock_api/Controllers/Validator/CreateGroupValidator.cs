using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class CreateGroupValidator : AbstractValidator<CreateGroupRequest>
    {
        public CreateGroupValidator()
        {
            RuleFor(x => x.GroupName).NotEmpty().WithMessage("groupName為必須");
            RuleFor(x => x.IsActive).NotEmpty().WithMessage("isActive為必須");
        }
    }
}
