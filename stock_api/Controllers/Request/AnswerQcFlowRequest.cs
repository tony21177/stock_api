namespace stock_api.Controllers.Request
{
    public class AnswerQcFlowRequest
    {
        public string? FlowId { get; set; }
        public string? ApplyId { get; set; }
        public string Answer { get; set; }

        public string? Reason { get; set; }
    }
}
