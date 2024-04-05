namespace stock_api.Controllers.Request
{
    public class UpdateMemberGroupRequest
    {
        public List<string> GroupIds { get; set; }

        public string UserId { get; set; }
    }
}
