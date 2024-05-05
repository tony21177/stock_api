using stock_api.Service.ValueObject;

namespace stock_api.Controllers.Request
{
    public class BaseSearchRequest
    {
        public string? Keywords { get; set; }

        public PaginationCondition PaginationCondition { get; set; } = new PaginationCondition();
    }
}
