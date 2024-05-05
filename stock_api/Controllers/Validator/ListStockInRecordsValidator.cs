using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class ListStockInRecordsValidator : AbstractValidator<ListStockInRecordsRequest>
    {
        public ListStockInRecordsValidator()
        {
            RuleFor(request => request.OutStockStatusList)
                 .Cascade(CascadeMode.Stop) // Stop on first failure
                 .Must(statusList => statusList.All(qcStatus => CommonConstants.OutStockStatus.GetAllValues().Contains(qcStatus)))
                 .When(request => request.OutStockStatusList!=null&&request.OutStockStatusList.Count>0) 
                     .WithMessage($"outStockStatusList必須為{string.Join(",", CommonConstants.OutStockStatus.GetAllValues())}");
        }
    }
}
