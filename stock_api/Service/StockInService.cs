using AutoMapper;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;

namespace stock_api.Service
{
    public class StockInService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public StockInService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public List<PurchaseAcceptanceItemsView> SearchPurchaseAcceptanceItems(SearchPurchaseAcceptItemRequest request)
        {
            IQueryable<PurchaseAcceptanceItemsView> query = _dbContext.PurchaseAcceptanceItemsViews;

            if (request.ReceiveStatus != null)
            {
                query = query.Where(h => h.ReceiveStatus == request.ReceiveStatus);
            }
            //if (request.DemandDateStart != null)
            //{
            //    DateOnly startDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(request.DemandDateStart).Value);
            //    query = query.Where(h => h.DemandDate >= startDate);
            //}
            //if (request.DemandDateEnd != null)
            //{
            //    DateOnly endDate = DateOnly.FromDateTime(DateTimeHelper.ParseDateString(request.DemandDateStart).Value).AddDays(1);
            //    query = query.Where(h => h.DemandDate < endDate);
            //}
            if (request.DemandDateStart != null)
            {
                var startDateTime = DateTimeHelper.ParseDateString(request.DemandDateStart).Value;
                query = query.Where(h => h.DemandDate.Value.ToDateTime(new TimeOnly(0,0)) >= startDateTime);
            }
            if (request.DemandDateEnd != null)
            {
                var endDateTime = DateTimeHelper.ParseDateString(request.DemandDateEnd).Value.AddDays(1);
                query = query.Where(h => h.DemandDate.Value.ToDateTime(new TimeOnly(0, 0)) < endDateTime);
            }
            if (request.ApplyDateStart != null)
            {
                query = query.Where(h => h.ApplyDate >= DateTimeHelper.ParseDateString(request.ApplyDateStart).Value);
            }
            if (request.ApplyDateEnd != null)
            {
                DateTime endDateTime =DateTimeHelper.ParseDateString(request.ApplyDateEnd).Value.AddDays(1);
                query = query.Where(h => h.ApplyDate < endDateTime);
            }
            if (request.GroupId != null)
            {
                query = query.Where(h => h.GroupIds.Contains(request.GroupId));
            }
            if (request.Type != null)
            {
                query = query.Where(h => h.Type == request.Type);
            }
            query = query.Where(h => h.CompId == request.CompId);
            if (!string.IsNullOrEmpty(request.Keywords))
            {
                query = query.Where(h =>
                h.Remarks.Contains(request.Keywords)
                || h.LotNumberBatch.Contains(request.Keywords)
                || h.LotNumber.Contains(request.Keywords)
                || h.ProductId.Contains(request.Keywords)
                || h.ProductName.Contains(request.Keywords)
                || h.ProductSpec.Contains(request.Keywords)
                || h.UdiserialCode.Contains(request.Keywords));
            }

            return query.ToList();
        }
    }
}
