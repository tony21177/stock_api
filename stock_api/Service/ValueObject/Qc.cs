namespace stock_api.Service.ValueObject
{
    public class Qc
    {
        public bool IsNeedQc { get; set; } = false;
        public string? QcType { get; set; }
        public Lot Lot { get; set; }= new ();
    }

    public class Lot
    {
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string LotNumber { get; set; } = null!;
        public string LotNumberBatch { get; set; } = null!;
    }
}
