using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Service;

namespace stock_api.Controllers.Validator
{
    public class GetProductInAndOutStockRecordsValidator : AbstractValidator<GetProductInAndOutStockRecordsRequest>
    {
        private readonly GroupService _groupService;

        public GetProductInAndOutStockRecordsValidator(GroupService groupService)
        {
            _groupService = groupService;
            
            RuleFor(x => x.GroupId)
                .Must((request, groupId, context) => BeValidGroupId(groupId, context))
                .WithMessage("該組別不存在");
            RuleFor(x => x.InStartDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.InEndDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.OutStartDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
            RuleFor(x => x.OutEndDate).Must((request, date, context) => BeValidDate(date, context))
                .WithMessage("無效格式日期");
        }

        private bool BeValidGroupId(string groupId, ValidationContext<GetProductInAndOutStockRecordsRequest> context)
        {
            var group = _groupService.GetGroupByGroupId(groupId); 

            if (group == null)
            {
                return false;
            }
            return true;
        }
        private static bool BeValidDate(string? date, ValidationContext<GetProductInAndOutStockRecordsRequest> context)
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
