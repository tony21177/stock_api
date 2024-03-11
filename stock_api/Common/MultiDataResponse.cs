namespace stock_api.Common
{
    public class MultiDataResponse<T, T2>
    {
        public bool Result { get; set; }
        public string Message { get; set; } = "";

        public T Data { get; set; }

        public T2 SubData { get; set; }
    }
}
