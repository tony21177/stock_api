using Google.Protobuf.WellKnownTypes;
using System;
using System.Runtime.InteropServices;
using System.Security.Policy;

namespace stock_api.Common.Constant
{
    public class CommonConstants
    {
        public static class EmailNotifyType
        {
            public const string PURCHASE = "PURCHASE";

            public static List<string> GetAllValues()
            {
                return new List<string> { PURCHASE };
            }
        }

        public static class CompanyType
        {
            public const string OWNER = "OWNER";
            public const string ORGANIZATION = "ORGANIZATION";
            public const string ORGANIZATION_NOSTOCK = "ORGANIZATION_NOSTOCK";

            public static List<string> GetAllValues()
            {
                return new List<string> { OWNER, ORGANIZATION, ORGANIZATION_NOSTOCK };
            }
        }


        public static class PurchaseApplyStatus
        {
            public const string APPLY = "APPLY";
            public const string AGREE = "AGREE";
            public const string REJECT = "REJECT";
            public const string BACK = "BACK";
            public const string CLOSE = "CLOSE";

            public static List<string> GetAllValues()
            {
                return new List<string> { APPLY, AGREE, REJECT, CLOSE };
            }
        }

        public static class ApplyNewProductCurrentStatus
        {
            public const string APPLY = "APPLY";
            public const string AGREE = "AGREE";
            public const string REJECT = "REJECT";
            public const string DONE = "DONE";
            public const string CLOSE = "CLOSE";

            public static List<string> GetAllValues()
            {
                return new List<string> { APPLY, AGREE, REJECT, CLOSE, DONE };
            }
        }


        public static class PurchaseMainOwnerProcessStatus
        {
            public const string NONE = "NONE";
            public const string NOT_AGREE = "NOT_AGREE";
            public const string PART_AGREE = "PART_AGREE";
            public const string AGREE = "AGREE";

            public static List<string> GetAllValues()
            {
                return new List<string> { NONE, NOT_AGREE, AGREE, PART_AGREE };
            }
        }

        public static class UpdateOwnerProcessStatus
        {
            public const string NOT_AGREE = "NOT_AGREE";
            public const string AGREE = "AGREE";

            public static List<string> GetAllValues()
            {
                return new List<string> {  NOT_AGREE, AGREE,  };
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

        public static class PurchaseCurrentStatus
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

        public static class ApplyNewProductFlowAnswer
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

        public static class AnswerApplyNewProductFlow
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

        public static class ApplyNewProductFlowStatus
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

        public static class AdjustType
        {
            public const string SHIFT = "SHIFT";
            public const string ADJUST = "ADJUST";
            public const string RETURN = "RETURN";
            public const string RETURN_OUT = "RETURN_OUT";

            public static List<string> GetAllValues()
            {
                return new List<string> { SHIFT, ADJUST, RETURN, RETURN_OUT };
            }
        }

        public static class AdjustStatus
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
        public static class InStockStatus
        {
            public const string NONE = "NONE";
            public const string PART = "PART";
            public const string DONE = "DONE";

            public static List<string> GetAllValues()
            {
                return new List<string> { NONE, PART, DONE };
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

        public static class QcTypeConstants
        {
            public const string NONE = "NONE";
            public const string LOT_NUMBER = "LOT_NUMBER";
            public const string LOT_NUMBER_BATCH = "LOT_NUMBER_BATCH";

            public static List<string> GetAllValues()
            {
                return new List<string> { NONE, LOT_NUMBER, LOT_NUMBER_BATCH };
            }
        }

        public static class QcTestStatus
        {
            public const string NONE = "NONE";
            public const string DONE = "DONE";

            public static List<string> GetAllValues()
            {
                return new List<string> { NONE, DONE };
            }
        }

        public static class AbnormalType
        {
            public const string RECEIVE_ABNORMAL = "RECEIVE_ABNORMAL";
            public const string VERIFY_ABNORMAL = "VERIFY_ABNORMAL";
            public const string QA_ABNORMAL = "QA_ABNORMAL";
            public const string OTHER_ABNORMAL = "OTHER_ABNORMAL";

            public static List<string> GetAllValues()
            {
                return new List<string> { RECEIVE_ABNORMAL, VERIFY_ABNORMAL, QA_ABNORMAL, OTHER_ABNORMAL };
            }
        }

        public static class SourceType
        {
            public const string IN_STOCK = "IN_STOCK";
            public const string OUT_STOCK = "OUT_STOCK";
            public const string QA = "QA";
            public const string MANUAL = "MANUAL";

            public static List<string> GetAllValues()
            {
                return new List<string> { IN_STOCK, OUT_STOCK, QA, MANUAL };
            }
        }

        public static class QcFinalResult
        {
            public const string PASS = "PASS";
            public const string FAIL = "FAIL";

            public static List<string> GetAllValues()
            {
                return new List<string> { PASS, FAIL };
            }
        }

        public static class NewLotNumberTestResult
        {
            public const string EXECUTED = "EXECUTED";
            public const string YES = "YES";
            public const string NO = "NO";

            public static List<string> GetAllValues()
            {
                return new List<string> { EXECUTED, YES, NO };
            }
        }

        public static class PreTestResult
        {
            public const string PASS = "PASS";
            public const string FAIL = "FAIL";
            public const string YES = "YES";

            public static List<string> GetAllValues()
            {
                return new List<string> { PASS, FAIL, YES };
            }
        }

        public static class PurchaseSubItemHistoryAction
        {
            public const string ADD = "ADD";
            public const string MODIFY = "MODIFY";
            public const string DELETE = "DELETE";

            public static List<string> GetAllValues()
            {
                return new List<string> { ADD, MODIFY, DELETE };
            }
        }

    }

    
}
