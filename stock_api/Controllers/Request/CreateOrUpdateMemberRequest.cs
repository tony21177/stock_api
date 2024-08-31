

namespace stock_api.Controllers.Request
{
    
    public class CreateOrUpdateMemberRequest
    {
        public string? UserId { get; set; }
        public string? Account { get; set; }
        public string? Password { get; set; }
        public string? DisplayName { get; set; }
        public short? AuthValue { get; set; }
        public string? PhotoUrls { get; set; }
        public string? Email { get; set; }
        public List<string>? GroupIds { get; set; } 
        public string? CompId { get; set; }
        public bool? IsActive { get; set; }

        public List<string>? Agents { get; set; }
    }
}