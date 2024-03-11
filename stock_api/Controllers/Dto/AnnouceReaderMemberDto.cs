using System.Text.Json.Serialization;

namespace stock_api.Controllers.Dto
{
    public class AnnouceReaderMemberDto
    {
        //public int Id { get; set; }
        public string Account { get; set; }
        public string DisplayName { get; set; }
        public string UserId { get; set; }
        public bool? IsActive { get; set; }
        public short? AuthValue { get; set; }
        public DateTime? CreatedTime { get; set; }
        [JsonPropertyName("photoUrls")]
        public string PhotoUrl { get; set; }
        public string Uid { get; set; }
        public DateTime? ReadTime { get; set; }
        public bool IsRead { get; set; }
    }
}
