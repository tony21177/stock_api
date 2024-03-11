namespace stock_api.Service.ValueObject
{
    public class Recipient
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Account { get; set; }
        public short AuthValue { get; set; }
        public string AuthName { get; set; }
        public string AuthDescription { get; set; }
    }
}
