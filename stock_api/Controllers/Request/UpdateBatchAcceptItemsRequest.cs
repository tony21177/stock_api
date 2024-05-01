using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Controllers.Request
{
    public class UpdateBatchAcceptItemsRequest
    {
        public List<UpdateAcceptItemRequest> UpdateAcceptItemList { get; set; }
    }

    public class UpdateAcceptItemRequest
    {
        public string AcceptId { get; set; }
        public int? AcceptQuantity { get; set; }
        public string? AcceptUserId { get; set; }
        public string? LotNumber { get; set; }
        public string? ExpirationDate { get; set; }
        public string? PackagingStatus { get; set; }
        public string? QcStatus { get; set; }
        public string? Comment { get; set; }
        public string? QcComment { get; set; }
    }
}
