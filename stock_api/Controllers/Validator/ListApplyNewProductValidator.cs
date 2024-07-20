using FluentValidation;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class ListApplyNewProductValidator : AbstractValidator<ListApplyNewProductMainRequest>
    {
        private readonly GroupService _groupService;
        

        public ListApplyNewProductValidator( GroupService groupService)
        {
            _groupService = groupService;

            RuleFor(x => x.StartDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.EndDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.ProductGroupId)
                .Must((request, groupId, context) => BeValidGroup(groupId, context))
                .WithMessage("該群組不存在");
           
            RuleFor(request => request.CurrentStatus)
                .Cascade(CascadeMode.Stop) // Stop on first failure
                .Must(status => CommonConstants.ApplyNewProductCurrentStatus.GetAllValues().Contains(status)).When(request => !string.IsNullOrEmpty(request.CurrentStatus)) // Only validate when Type is not empty
                    .WithMessage($"currentStatus必須為{string.Join(",", CommonConstants.ApplyNewProductCurrentStatus.GetAllValues())}");

        }


        private static bool BeValidDate(string? date, ValidationContext<ListApplyNewProductMainRequest> context)
        {
            if (date == null)
            {
                return true;
            }

            if (DateTimeHelper.ParseDateString(date) != null) return true;
            return false;
        }

        private bool BeValidGroup(string? groupId, ValidationContext<ListApplyNewProductMainRequest> context)
        {
            if (groupId == null)
            {
                return true; 
            }

            var group = _groupService.GetGroupByGroupId(groupId);
            if (group == null) return false;
            return true;
        }
    }
}
