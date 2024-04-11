﻿namespace stock_api.Controllers.Request
{
    public class ListPurchaseRequest
    {
        public string? CompId { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? GroupId { get; set; }
        public string? Type { get; set; }
        public string? CurrentStatus { get; set; }
        public string? ReceiveStatus { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsNeedFlow { get; set; }

     }
}
