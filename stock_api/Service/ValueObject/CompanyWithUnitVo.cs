namespace stock_api.Service.ValueObject
{
    public class CompanyWithUnitVo
    {
        public string CompId { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
    }
}
