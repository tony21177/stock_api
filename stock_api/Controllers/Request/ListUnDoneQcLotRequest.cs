namespace stock_api.Controllers.Request
{
    public class ListUnDoneQcLotRequest: BaseSearchRequest
    {
        public string? CompId { get; set; }
        public string? GroupId { get; set; }
    }
}
