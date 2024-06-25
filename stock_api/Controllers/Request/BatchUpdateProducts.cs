using stock_api.Service.ValueObject;

namespace stock_api.Controllers.Request
{
    public class BatchUpdateProducts
    {
        public List<ModifyProductDto> ModifyProductDtoList { get; set; } = null!;
    }
}
