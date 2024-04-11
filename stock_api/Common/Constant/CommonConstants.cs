using Google.Protobuf.WellKnownTypes;
using System;
using System.Runtime.InteropServices;
using System.Security.Policy;

namespace stock_api.Common.Constant
{
    public class CommonConstants
    {
        public static class CompanyType
        {
            public const string OWNER = "OWNER";
            public const string ORGANIZATION = "ORGANIZATION";

            public static List<string> GetAllValues()
            {
                return new List<string> { OWNER, ORGANIZATION };
            }
        }


        public static class PurchaseApplyStatus
        {
            public const string APPLY = "APPLY";
            public const string AGREE = "AGREE";
            public const string REJECT = "REJECT";
            public const string CLOSE = "CLOSE";

            public static List<string> GetAllValues()
            {
                return new List<string> { APPLY, AGREE, REJECT, CLOSE };
            }
        }

        public static class PurchaseReceiveStatus
        {
            public const string NONE = "NONE";
            public const string DELIVERED = "DELIVERED";
            public const string IN_ACCEPTANCE_CHECK = "IN_ACCEPTANCE_CHECK";
            public const string PART_ACCEPT = "PART_ACCEPT";
            public const string ALL_ACCEPT = "ALL_ACCEPT";
            public static List<string> GetAllValues()
            {
                return new List<string> { NONE, DELIVERED, IN_ACCEPTANCE_CHECK, PART_ACCEPT, ALL_ACCEPT };
            }
        }

        public static class PurchaseFlowStatus
        {
            public const string WAIT = "WAIT";
            public const string DONE = "DONE";
            public static List<string> GetAllValues()
            {
                return new List<string> { WAIT, DONE };
            }
        }

        public static class PurchaseFlowAnswer
        {
            public const string AGREE = "AGREE";
            public const string REJECT = "REJECT";
            public const string EMPTY = "";
            public static List<string> GetAllValues()
            {
                return new List<string> { AGREE, REJECT, EMPTY };
            }
        }

        public static class PurchaseType
        {
            public const string GENERAL = "GENERAL";
            public const string URGENT = "URGENT";

            public static List<string> GetAllValues()
            {
                return new List<string> { GENERAL, URGENT };
            } 
        }
    }

    
}
