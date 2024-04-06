namespace stock_api.Service.ValueObject
{
    public class FileDetail
    {
        public string? AttId { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }

        public string? FileType { get; set; }
        public string? FileSizeText { get; set; }
        public long? FileSizeNumber { get; set; }
        public long? CreatedAt { get; set; }
        public long? UpdatedTime { get; set; }
    }
}
