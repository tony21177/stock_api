using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;

namespace stock_api.Service
{
    public class SupplierTraceService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public SupplierTraceService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public void CreateSupplierTrace(SupplierTraceLog newSupplierTraceLog)
        {
            _dbContext.SupplierTraceLogs.Add(newSupplierTraceLog);
            _dbContext.SaveChanges();
        }

        public void UpdateSupplierTrace(SupplierTraceLog updateSupplierTraceLog, SupplierTraceLog existingSupplierTraceLog)
        {
            _mapper.Map(updateSupplierTraceLog, existingSupplierTraceLog);

            _dbContext.SaveChanges();
            return;
        }

        public void DeleteSupplierTrace(int id)
        {
            _dbContext.SupplierTraceLogs.Where(l => l.Id == id).ExecuteDelete();
            _dbContext.SaveChanges();
            return;
        }

        public SupplierTraceLog? GetById(int id)
        {
            return _dbContext.SupplierTraceLogs.Where(l => l.Id == id).FirstOrDefault();
        }

        public (List<SupplierTraceLog>,int) ListSupplierTraceLog(ListSupplierTraceLogRequest listSupplierTraceLogRequest)
        {
            IQueryable<SupplierTraceLog> query = _dbContext.SupplierTraceLogs;
            if (listSupplierTraceLogRequest.CompId != null)
            {
                query = query.Where(h => h.CompId == listSupplierTraceLogRequest.CompId);
            }

            if (listSupplierTraceLogRequest.SourceType != null)
            {
                query = query.Where(h => h.SourceType == listSupplierTraceLogRequest.SourceType);
            }

            if (listSupplierTraceLogRequest.AbnormalType != null)
            {
                query = query.Where(h => h.AbnormalType == listSupplierTraceLogRequest.AbnormalType);
            }
            if (listSupplierTraceLogRequest.InDays != null)
            {
                var afterDateInDays = DateTime.Now.AddDays(listSupplierTraceLogRequest.InDays.Value + 1).Date;
                query = query.Where(h => h.CreatedAt < afterDateInDays);
            }
            if (listSupplierTraceLogRequest.ProductId != null)
            {
                query = query.Where(h => h.ProductId==listSupplierTraceLogRequest.ProductId);
            }
            if(listSupplierTraceLogRequest.SupplierId != null)
            {
                query = query.Where(h => h.SupplierId == listSupplierTraceLogRequest.SupplierId);
            }

            if (!string.IsNullOrEmpty(listSupplierTraceLogRequest.Keywords))
            {
                var groupNameList =
                query = query.Where(h => (h.ProductName != null && h.ProductName.Contains(listSupplierTraceLogRequest.Keywords))
                || (h.SourceType != null && h.SourceType.Contains(listSupplierTraceLogRequest.Keywords))
                || (h.SupplierName != null && h.SupplierName.Contains(listSupplierTraceLogRequest.Keywords))
                || (h.AbnormalContent != null && h.AbnormalContent.Contains(listSupplierTraceLogRequest.Keywords))
                || (h.AbnormalType != null && h.AbnormalType.Contains(listSupplierTraceLogRequest.Keywords)));
            }

            if (listSupplierTraceLogRequest.PaginationCondition.OrderByField == null) listSupplierTraceLogRequest.PaginationCondition.OrderByField = "UpdatedAt";
            if (listSupplierTraceLogRequest.PaginationCondition.IsDescOrderBy)
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(listSupplierTraceLogRequest.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "AbnormalDate" => query.OrderByDescending(h => h.AbnormalDate),
                    "CreatedAt" => query.OrderByDescending(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderByDescending(h => h.UpdatedAt),
                    _ => query.OrderByDescending(h => h.UpdatedAt),
                };
            }
            else
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(listSupplierTraceLogRequest.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "AbnormalDate" => query.OrderByDescending(h => h.AbnormalDate),
                    "CreatedAt" => query.OrderByDescending(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderByDescending(h => h.UpdatedAt),
                    _ => query.OrderBy(h => h.UpdatedAt),
                };
            }
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / listSupplierTraceLogRequest.PaginationCondition.PageSize);

            query = query.Skip((listSupplierTraceLogRequest.PaginationCondition.Page - 1) * listSupplierTraceLogRequest.PaginationCondition.PageSize).Take(listSupplierTraceLogRequest.PaginationCondition.PageSize);
            return (query.ToList(), totalPages);

        }


        public (List<SupplierTraceLog>, int) ReportListSupplierTraceLog(ReportListSupplierTraceLogRequest request)
        {
            IQueryable<SupplierTraceLog> query = _dbContext.SupplierTraceLogs;
            query = query.Where(h => h.CompId == request.CompId);

            if (request.SupplierId != null)
            {
                query = query.Where(h => h.SupplierId == request.SupplierId);
            }

            if (request.StartDate != null)
            {
                query = query.Where(h => h.AbnormalDate!=null&&h.AbnormalDate >= DateTimeHelper.ParseDateString(request.StartDate).Value);
            }
            if (request.EndDate != null)
            {
                DateTime endDateTime = DateTimeHelper.ParseDateString(request.EndDate).Value.AddDays(1);
                query = query.Where(h => h.AbnormalDate != null && h.AbnormalDate < endDateTime);
            }


            if (request.PaginationCondition.OrderByField == null) request.PaginationCondition.OrderByField = "AbnormalDate";
            if (request.PaginationCondition.IsDescOrderBy)
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "AbnormalDate" => query.OrderByDescending(h => h.AbnormalDate),
                    "CreatedAt" => query.OrderByDescending(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderByDescending(h => h.UpdatedAt),
                    _ => query.OrderByDescending(h => h.UpdatedAt),
                };
            }
            else
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "AbnormalDate" => query.OrderByDescending(h => h.AbnormalDate),
                    "CreatedAt" => query.OrderByDescending(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderByDescending(h => h.UpdatedAt),
                    _ => query.OrderBy(h => h.UpdatedAt),
                };
            }
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);

            query = query.Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize).Take(request.PaginationCondition.PageSize);
            return (query.ToList(), totalPages);

        }
    }
}
