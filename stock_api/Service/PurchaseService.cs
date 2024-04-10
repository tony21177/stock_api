using AutoMapper;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.Transactions;

namespace stock_api.Service
{
    public class PurchaseService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(StockDbContext dbContext, IMapper mapper, ILogger<PurchaseService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }


        public bool CreatePurchase(PurchaseMainSheet newPurchasePurchaseMainSheet,List<Models.PurchaseSubItem> purchaseSubItemList, List<PurchaseFlowSettingVo> purchaseFlowSettingList)
        {
            using (var scope = new TransactionScope())
            {
                try
                {
                    var purchaseMainId = Guid.NewGuid().ToString();
                    newPurchasePurchaseMainSheet.PurchaseMainId = purchaseMainId;
                    newPurchasePurchaseMainSheet.CurrentStatus = CommonConstants.PurchaseApplyStatus.APPLY;
                    newPurchasePurchaseMainSheet.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.NONE;
                    newPurchasePurchaseMainSheet.IsActive = true;
                    _dbContext.PurchaseMainSheets.Add(newPurchasePurchaseMainSheet);

                    foreach (var item in purchaseSubItemList)
                    {
                        item.ItemId = Guid.NewGuid().ToString();
                        item.PurchaseMainId = purchaseMainId;
                        item.ReceiveStatus = CommonConstants.PurchaseReceiveStatus.NONE;
                    }
                    _dbContext.PurchaseSubItems.AddRange(purchaseSubItemList);


                    List<PurchaseFlow> purchaseFlows = new List<PurchaseFlow>();
                    DateTime submitedAt = DateTime.Now;
                    foreach (var item in purchaseFlowSettingList)
                    {
                        var purchaseFlow = new PurchaseFlow()
                        {
                            FlowId = Guid.NewGuid().ToString(),
                            CompId = newPurchasePurchaseMainSheet.CompId,
                            PurchaseMainId = purchaseMainId,
                            Status = CommonConstants.PurchaseFlowStatus.WAIT,
                            VerifyCompId = newPurchasePurchaseMainSheet.CompId,
                            VerifyUserId = item.UserId,
                            VerifyUserName = item.UserDisplayName,
                            Answer = CommonConstants.PurchaseFlowAnswer.EMPTY,
                            Sequence = item.Sequence,
                            SubmitAt = submitedAt,
                        };
                        purchaseFlows.Add(purchaseFlow);
                    }
                    _dbContext.PurchaseFlows.AddRange(purchaseFlows);
                    _dbContext.SaveChanges();
                    scope.Complete();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError("事務失敗[CreatePurchaseRequest]：{msg}", ex);
                    return false;
                }
            }

        }
    }
}
