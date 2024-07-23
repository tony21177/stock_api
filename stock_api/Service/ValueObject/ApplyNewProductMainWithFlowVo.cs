using stock_api.Models;

namespace stock_api.Service.ValueObject
{
    public class ApplyNewProductMainWithFlowVo:ApplyNewProductMain
    {

        public List<ApplyNewProductFlow> Flows { get; set; } = new();
        public string? ApplyUserName {  get; set; }
        public string? ApplyCompName { get; set;}
        public string? ApplyCompUnitName { get; set;}
    }
}
