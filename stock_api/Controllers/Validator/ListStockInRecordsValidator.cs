using FluentValidation;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;

namespace stock_api.Controllers.Validator
{
    public class ListStockInRecordsValidator : AbstractValidator<ListStockInRecordsRequest>
    {
        public ListStockInRecordsValidator()
        {
           
        }
    }
}
