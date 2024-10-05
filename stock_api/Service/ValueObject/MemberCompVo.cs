using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class MemberCompVo: WarehouseMember
    {
        public string CompName { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
