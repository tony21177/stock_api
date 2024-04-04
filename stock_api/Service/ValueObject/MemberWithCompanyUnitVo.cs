using stock_api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace stock_api.Service.ValueObject
{
    public class MemberWithCompanyUnitVo : WarehouseMember
    {

        [Column("CompanyName")]
        [JsonPropertyName("companyName")]
        public string Name { get; set; }
        [Column("CompanType")]
        [JsonPropertyName("companType")]
        public string Type { get; set; }
        [Column("UnitId")]
        [JsonPropertyName("unitId")]
        public string UnitId { get; set; }
        [Column("UnitName")]
        [JsonPropertyName("nnitName")]
        public string UnitName { get; set; }

        public List<string> GroupIds { get; set; }
        public List<WarehouseGroup> Groups { get; set; }
    }
}
