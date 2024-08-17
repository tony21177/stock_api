using stock_api.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace stock_api.Service.ValueObject
{
    public class PurchaseFlowWithAgentsVo:PurchaseFlow
    {
        public List<string> VerifyAgentIds { get; set; } = new List<string>();
        public List<string> VerifyAgentNames { get; set; } = new List<string>();
        [JsonIgnore]
        [Column("Agents")]
        public string? Agents {  get; set; }
        [JsonIgnore]
        [Column("AgentNames")]
        public string? AgentNames { get; set; }
    }
}
