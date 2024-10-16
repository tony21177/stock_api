﻿using AutoMapper;
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

        public List<ProductInAndOutRecord> GetProductInAndOutRecords(WarehouseGroup group,List<WarehouseProduct> products, GetProductInAndOutStockRecordsRequest request)
        {
            var productIdList = products.Select(p => p.ProductId).ToList();

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
            var resultOutStockRecors = outStockQuery.ToList();

            var inStockRecordVoList = _mapper.Map<List<InStockItemRecordVo>>(resultInStockRecords);
            inStockRecordVoList.ForEach(vo =>
            {
                vo.GroupIds = group.GroupId;
                vo.GroupNames = group.GroupName;
            });
            inStockRecordVoList = inStockRecordVoList.OrderBy(vo => vo.CreatedAt).ToList();
            var outStockRecordVoList = _mapper.Map<List<OutStockRecordVo>>(resultOutStockRecors);
            foreach (var item in outStockRecordVoList)
            {
                var matchedProdcut = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();
                item.Unit = matchedProdcut?.Unit;
                item.OpenDeadline = matchedProdcut?.OpenDeadline ?? 0;
            }
            outStockRecordVoList = outStockRecordVoList.OrderBy(vo => vo.CreatedAt).ToList();

            List<ProductInAndOutRecord> productInAndOutRecords = new List<ProductInAndOutRecord>();

            foreach (WarehouseProduct product in products)
            {
                var productInAndOutRecord = new ProductInAndOutRecord()
                {
                    ProductName = product.ProductName,
                    ProductCode = product.ProductCode,
                    InStockItemRecords = inStockRecordVoList.Where(i=>i.ProductId==product.ProductId).ToList(),
                    OutStockRecords = outStockRecordVoList.Where(i => i.ProductId == product.ProductId).ToList(),
                };
                productInAndOutRecords.Add(productInAndOutRecord);
            }
            productInAndOutRecords = productInAndOutRecords.OrderBy(p=>p.ProductCode).ToList();
            return productInAndOutRecords;
        }
    }
}
