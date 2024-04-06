namespace stock_api.Controllers.Request
{
    public class UpdateAuthlayerRequest
    {
        public int AuthId { get; set; }
        public string? AuthName { get; set; }
        public short? AuthValue { get; set; }
        public string? AuthDescription { get; set; }

        public bool? IsApplyItemManage { get; set; }

        public bool? IsGroupManage { get; set; }
        public bool? IsInBoundManage { get; set; }
        public bool? IsOutBoundManage { get; set; }
        public bool? IsInventoryManage { get; set; }
        public bool? IsItemManage { get; set; }
        public bool? IsMemberManage { get; set; }
        public bool? IsRestockManage { get; set; }
        public bool? IsVerifyManage { get; set; }
    }
}
