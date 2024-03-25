using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class UpdateGroupValidator : AbstractValidator<UpdateGroupRequest>
    {
        public UpdateGroupValidator()
        {
            RuleFor(x => x.GroupId).NotEmpty().WithMessage("groupId為必須");
        }
    }
}
