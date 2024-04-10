namespace stock_api.Common.Constant
{
    public class CommonConstants
    {
        public static class CompanyType
        {
            public const string Owner = "OWNER";
            public const string Organization = "ORGANIZATION";
        }


        public static class PurchaseApplyStatus
        {
            public const string APPLY = "APPLY";
            public const string AGREE = "AGREE";
            public const string REJECT = "REJECT";
            public const string CLOSE = "CLOSE";
        }

        public static class PurchaseReceiveStatus
        {
            public const string NONE = "NONE";
            public const string DELIVERED = "DELIVERED";
            public const string IN_ACCEPTANCE_CHECK = "IN_ACCEPTANCE_CHECK";
            public const string PART_ACCEPT = "PART_ACCEPT";
            public const string ALL_ACCEPT = "ALL_ACCEPT";
        }

        public static class PurchaseFlowStatus
        {
            public const string WAIT = "WAIT";
            public const string DONE = "DONE";
        }

        public static class PurchaseFlowAnswer
        {
            public const string AGREE = "AGREE";
            public const string REJECT = "REJECT";
            public const string EMPTY = "";
        }
    }

    
}
