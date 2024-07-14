namespace stock_api.Service.ValueObject
{
    public class PaginationCondition
    {
        public int PageSize { get; set; } = 1000;

        public int Page { get; set; } = 1;

        public string? OrderByField { get; set; }

        public bool IsDescOrderBy { get; set; } = true;

    }
}
