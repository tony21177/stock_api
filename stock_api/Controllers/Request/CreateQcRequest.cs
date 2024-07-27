using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class CreateQcRequest
    {
        public string? CompId {  get; set; }
        public string? LotNumber { get; set; }
        public string? LotNumberBatch { get; set; }
        public string? ValidationType { get; set; }

        public string? ValidationMethod { get; set; }

        public string? ValidationItemName { get; set; }

        public string? Comment { get; set; }
        public string? QcType {  get; set; }

        public List<QcDetail> Details { get; set; } = null!;

    }
    public class QcDetail
    {
        public int ItemNumber { get; set; }
      
        public string? NewLotResult { get; set; }

        public string? OldLotResult { get; set; }

        public string? QuantityDiff { get; set; }
        public string? ValidationResult { get; set; }
    }
}
