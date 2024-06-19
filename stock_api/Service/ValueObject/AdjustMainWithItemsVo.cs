using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stock_api.Service.ValueObject
{
    public class AdjustMainWithItemsVo
    {
        public string MainId { get; set; } = null!;
        public string CompId { get; set; } = null!;
        public string? AdjustCompId { get; set; }
        public string Type { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string CurrentStatus { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<AdjustItemVo> Items { get; set; } = new List<AdjustItemVo>();
    }

    public class AdjustItemVo
    {
        public string AdjustItemId { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public float BeforeQuantity { get; set; }
        public float AfterQuantity { get; set; }
        public DateTime? ItemCreatedAt { get; set; }
        public DateTime? ItemUpdatedAt { get; set; }
    }
}
