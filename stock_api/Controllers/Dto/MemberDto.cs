using stock_api.Models;
using System.Text.Json.Serialization;

namespace stock_api.Controllers.Dto
{
    public class MemberDto
    {
        //public int Id { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
        public bool? IsActive { get; set; }
        public short? AuthValue { get; set; }
        public DateTime? CreatedAt { get; set; }
        [JsonPropertyName("photoUrls")]
        public string PhotoUrl { get; set; }
        public string Uid { get; set; }
        public string? Email { get; set; }

        public List<string> GroupIds {  get; set; }

        public List<WarehouseGroup> Groups { get; set; }
    }
}
