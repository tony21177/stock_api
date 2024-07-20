using FluentValidation;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class CreateApplyProductValidator : AbstractValidator<CreateApplyProductMainRequest>
    {
        private readonly GroupService _groupService;

        public CreateApplyProductValidator(GroupService groupService)
        {
            _groupService = groupService;

            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.ProductGroupId)
                .Must((request, groupId, context) => BeValidGroup(groupId, context))
                .WithMessage("無效的productGroupId");

        }


        private bool BeValidGroup(string groupId, ValidationContext<CreateApplyProductMainRequest> context)
        {

            var group = _groupService.GetGroupByGroupId(groupId);
            if(group==null||group.IsActive==false) return false;
            return true;
        }

    }

}
