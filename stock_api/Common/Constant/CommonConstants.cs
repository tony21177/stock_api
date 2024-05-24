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

        public static class PurchaseSubItemReceiveStatus
        {
            public const string NONE = "NONE";
            public const string PART = "PART";
            public const string DONE = "DONE";
            public const string CLOSE = "CLOSE";
            public static List<string> GetAllValues()
            {
                return new List<string> { NONE, PART, DONE, CLOSE };
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

        public static class AnswerPurchaseFlow
        {
            public const string AGREE = "AGREE";
            public const string REJECT = "REJECT";
            public const string BACK = "BACK";
            public static List<string> GetAllValues()
            {
                return new List<string> { AGREE, REJECT, BACK };
            }
        }

        public static class PurchaseFlowStatus
        {
            public const string WAIT = "WAIT";
            public const string AGREE = "AGREE";
            public const string REJECT = "REJECT";
            public static List<string> GetAllValues()
            {
                return new List<string> { WAIT, AGREE, REJECT };
            }
        }

        public static class PurchaseFlowLogAction
        {
            public const string AGREE = "AGREE";
            public const string REJECT = "REJECT";
            public const string MODIFY = "MODIFY";

            public static List<string> GetAllValues()
            {
                return new List<string> {  AGREE, REJECT, MODIFY };
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

        public static class QcStatus
        {
            public const string PASS = "PASS";
            public const string FAIL = "FAIL";
            public const string NONEED = "NONEED";
            public const string OTHER = "OTHER";

            public static List<string> GetAllValues()
            {
                return new List<string> { PASS, FAIL, NONEED, OTHER };
            }
        }

        public static class PackagingStatus
        {
            public const string NORMAL = "NORMAL";
            public const string BREAK = "BREAK";
            

            public static List<string> GetAllValues()
            {
                return new List<string> { NORMAL, BREAK };
            }
        }

        public static class StockInType
        {
            public const string PURCHASE = "PURCHASE";
            public const string SHIFT = "SHIFT";
            public const string ADJUST = "ADJUST";
            public const string RETURN = "RETURN";

            public static List<string> GetAllValues()
            {
                return new List<string> { PURCHASE, SHIFT, ADJUST, RETURN };
            }
        }

        public static class DeliverFunctionType
        {
            public const string NORMAL = "NORMAL";
            public const string REFRIGERATE = "REFRIGERATE";
            public const string FREEZED = "FREEZED";
            public const string OTHER = "OTHER";

            public static List<string> GetAllValues()
            {
                return new List<string> { NORMAL, REFRIGERATE, FREEZED, OTHER };
            }
        }

        public static class SavingFunctionType
        {
            public const string NORMAL = "NORMAL";
            public const string REFRIGERATE = "REFRIGERATE";
            public const string FREEZED = "FREEZED";
            public const string OTHER = "OTHER";

            public static List<string> GetAllValues()
            {
                return new List<string> { NORMAL, REFRIGERATE, FREEZED, OTHER };
            }
        }

        public static class OutStockStatus
        {
            public const string NONE = "NONE";
            public const string PART = "PART";
            public const string ALL = "ALL";

            public static List<string> GetAllValues()
            {
                return new List<string> { NONE, PART, ALL };
            }
        }

        public static class OutStockType
        {
            public const string PURCHASE_OUT = "PURCHASE_OUT";
            public const string SHIFT_OUT = "SHIFT_OUT";
            public const string ADJUST_OUT = "ADJUST_OUT";
            public const string RETURN_OUT = "RETURN_OUT";

            public static List<string> GetAllValues()
            {
                return new List<string> { PURCHASE_OUT, SHIFT_OUT, ADJUST_OUT, RETURN_OUT };
            }
        }

        public static class SplitProcess
        {
            public const string NONE = "NONE";
            public const string PART = "PART";
            public const string DONE = "DONE";

            public static List<string> GetAllValues()
            {
                return new List<string> { NONE, PART, DONE };
            }
        }

    }

    
}
