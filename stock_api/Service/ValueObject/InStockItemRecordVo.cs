using stock_api.Models;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Service.ValueObject
{
    public class InStockItemRecordVo: InStockItemRecord
    {
        public string? GroupIds { get; set; }
        public string? GroupNames { get; set; }
    }
}
