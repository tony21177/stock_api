using AutoMapper;
using Serilog;
using stock_api.Common.Constant;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.Linq;
using System.Security.AccessControl;
using System.Transactions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace stock_api.Service
{
    public class ReportService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;

        public ReportService(StockDbContext dbContext, IMapper mapper, ILogger<ReportService> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public List<ProductInAndOutRecord> GetProductInAndOutRecords(WarehouseGroup? group, GetProductInAndOutStockRecordsRequest request)
        {
            var allProducts = _dbContext.WarehouseProducts.Where(p=>p.CompId==request.CompId).ToList();
            var matchedProducts = allProducts;
            var allGroups = _dbContext.WarehouseGroups.ToList();
            if (group != null)
            {
                matchedProducts = allProducts.Where(p=>p.GroupIds!=null && p.GroupIds.Contains(group.GroupId)).ToList();
            }


            var productIdList = matchedProducts.Select(p => p.ProductId).ToList();

            IQueryable<InStockItemRecord> inStockQuery = _dbContext.InStockItemRecords;
            IQueryable<OutStockRecord> outStockQuery = _dbContext.OutStockRecords;
            inStockQuery = inStockQuery.Where(h => productIdList.Contains(h.ProductId));
            outStockQuery = outStockQuery.Where(h => productIdList.Contains(h.ProductId));


            if (request.InStartDate != null)
            {
                inStockQuery = inStockQuery.Where(h => h.CreatedAt >= DateTimeHelper.ParseDateString(request.InStartDate).Value);
            }
            if (request.InEndDate != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(request.InEndDate).Value.AddDays(1);
                inStockQuery = inStockQuery.Where(h => h.CreatedAt < endDateTime);
            }

            if (request.OutStartDate != null)
            {
                outStockQuery = outStockQuery.Where(h => h.CreatedAt >= DateTimeHelper.ParseDateString(request.OutStartDate).Value);
            }
            if (request.OutEndDate != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(request.OutEndDate).Value.AddDays(1);
                outStockQuery = outStockQuery.Where(h => h.CreatedAt < endDateTime);
            }
            var resultInStockRecords = inStockQuery.ToList();
            var resultOutStockRecords = outStockQuery.ToList();

            var inStockRecordVoList = _mapper.Map<List<InStockItemRecordVo>>(resultInStockRecords);
            inStockRecordVoList.ForEach(vo =>
            {
                if (group != null)
                {
                    vo.GroupIds = group.GroupId;
                    vo.GroupNames = group.GroupName;
                }
                else
                {
                    var matchedProduct = allProducts.Where(p => p.ProductId == vo.ProductId).FirstOrDefault();
                    vo.GroupIds = matchedProduct.GroupIds;
                    vo.GroupNames = matchedProduct.GroupNames;

                }

                
            });
            inStockRecordVoList = inStockRecordVoList.OrderBy(vo => vo.CreatedAt).ToList();
            var outStockRecordVoList = _mapper.Map<List<OutStockRecordVo>>(resultOutStockRecords);

            var allReturnStockRecordList = _dbContext.ReturnStockRecords.OrderByDescending(r => r.CreatedAt).ToList();
            var matchedReturnRecords = allReturnStockRecordList.Where(r => r.OutStockId == item.OutStockId).ToList();
            


            foreach (var item in outStockRecordVoList)
            {
                var matchedProduct = matchedProducts.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                item.Unit = matchedProduct?.Unit;
                item.OpenDeadline = matchedProduct?.OpenDeadline ?? 0;

                var returnInfoList = matchedReturnRecords.Select(r => new ReturnStockInfo
                {
                    ReturnQuantity = r.ReturnQuantity.Value,
                    OutStockApplyQuantityBefore = r.OutStockApplyQuantityBefore,
                    OutStockApplyQuantityAfter = r.OutStockApplyQuantityAfter,
                    AfterQuantityBefore = r.AfterQuantityBefore.Value,
                    AfterQuantityAfter = r.AfterQuantityAfter.Value,
                    ReturnStockDateTime = r.CreatedAt.Value,
                }).ToList();
                item.ReturnStockInfoList = returnInfoList;
            }
            outStockRecordVoList = outStockRecordVoList.OrderBy(vo => vo.CreatedAt).ToList();

            List<ProductInAndOutRecord> productInAndOutRecords = new List<ProductInAndOutRecord>();

            foreach (WarehouseProduct product in matchedProducts)
            {

                var productInAndOutRecord = new ProductInAndOutRecord()
                {
                    ProductName = product.ProductName,
                    ProductCode = product.ProductCode,
                    InStockItemRecords = inStockRecordVoList.Where(i=>i.ProductId==product.ProductId).ToList(),
                    OutStockRecords = outStockRecordVoList.Where(i => i.ProductId == product.ProductId).ToList(),
                };
                if (productInAndOutRecord.InStockItemRecords.Count != 0 || productInAndOutRecord.OutStockRecords.Count != 0)
                {
                    productInAndOutRecords.Add(productInAndOutRecord);

                }

            }
            productInAndOutRecords = productInAndOutRecords.OrderBy(p=>p.ProductCode).ToList();
            return productInAndOutRecords;
        }
    }
}
