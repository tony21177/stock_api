using AutoMapper;
using stock_api.Common.Utils;
using stock_api.Controllers.Request;
using stock_api.Models;

namespace stock_api.Service
{
    public class InstrumentService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public InstrumentService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public void AddInstrument(CreateInstrumentRequest request, WarehouseMember createMember)
        {
            Instrument newInstrument = new()
            {
                CompId = request.CompId,
                InstrumentName = request.InstrumentName,
                IsActive = request.IsActive,
            };
            newInstrument.CreateUserId = createMember.UserId;
            newInstrument.CreateUserName = createMember.DisplayName;
            _dbContext.Add(newInstrument);
            _dbContext.SaveChanges();

        }

        public void UpdateInstrument(UpdateInstrumentRequest request,Models.Instrument instrument)
        {
            if (request.InstrumentName != null)
            {
                instrument.InstrumentName = request.InstrumentName; 
            }
            if (request.IsActive != null)
            {
                instrument.IsActive = request.IsActive;
            }
            
            _dbContext.SaveChanges();
        }

        public List<Instrument> GetAll(string compId)
        {
            return _dbContext.Instruments.Where(i=>i.CompId==compId).ToList();
        }

        public Instrument? GetById(int instrumentId)
        {
            return _dbContext.Instruments.Where(i=>i.InstrumentId==instrumentId).FirstOrDefault();
        }

        public List<Instrument> GetByIdList(List<int> instrumentIds)
        {
            return _dbContext.Instruments.Where(i => instrumentIds.Contains( i.InstrumentId)).ToList();
        }

        public (List<Instrument>,int) ListInstrument(ListInstrumentRequest request)
        {
            IQueryable<Instrument> query = _dbContext.Instruments;
            if (request.CompId != null)
            {
                query = query.Where(e => e.CompId == request.CompId);
            }
            if (request.InstrumentId != null)
            {
                query = query.Where(e => e.InstrumentId == request.InstrumentId);
            }
            if (request.InstrumentName != null)
            {
                query = query.Where(e => e.InstrumentName == request.InstrumentName);
            }
            if (request.IsActive != null)
            {
                query = query.Where(e => e.IsActive == request.IsActive);
            }


            if (!string.IsNullOrEmpty(request.Keywords))
            {
                query = query.Where(h => h.InstrumentName.Contains(request.Keywords));
            }
            int totalPages = 0;
            if (request.PaginationCondition.OrderByField == null) request.PaginationCondition.OrderByField = "UpdatedAt";
            if (request.PaginationCondition.IsDescOrderBy)
            {
                var orderByField = StringUtils.CapitalizeFirstLetter(request.PaginationCondition.OrderByField);
                query = orderByField switch
                {
                    "InstrumentId" => query.OrderByDescending(h => h.InstrumentId),
                    "InstrumentName" => query.OrderByDescending(h => h.InstrumentName),
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
                    "InstrumentId" => query.OrderBy(h => h.InstrumentId),
                    "InstrumentName" => query.OrderBy(h => h.InstrumentName),
                    "CreatedAt" => query.OrderBy(h => h.CreatedAt),
                    "UpdatedAt" => query.OrderBy(h => h.UpdatedAt),
                    _ => query.OrderBy(h => h.UpdatedAt),
                };
            }
            int totalItems = query.Count();
            totalPages = (int)Math.Ceiling((double)totalItems / request.PaginationCondition.PageSize);
            query = query.Skip((request.PaginationCondition.Page - 1) * request.PaginationCondition.PageSize).Take(request.PaginationCondition.PageSize);
            var data = query.ToList();
            return (data, totalPages);
        }
    }
}
