﻿namespace stock_api.Controllers.Request
{
    public class ListStockInRecordsRequest : BaseSearchRequest
    {
        public string? CompId { get; set; }
        public string? LotNumberBatch { get; set;}
        public string? LotNumber {  get; set; }
        public string? ItemId { get; set; }
        public string? ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? Type { get; set;}
        public string? UserId { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }    
        public int? SupplierId { get; set; }

        public List<string>? OutStockStatusList { get; set; }
    }
}
