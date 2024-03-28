using AutoMapper;
using stock_api.Controllers.Request;
using stock_api.Models;

namespace stock_api.Service
{
    public class ManufacturerService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public ManufacturerService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public void AddManufacturer(Manufacturer manufacturer)
        {
            manufacturer.Id = Guid.NewGuid().ToString();
            _dbContext.Manufacturers.Add(manufacturer);
            _dbContext.SaveChanges();
        }

        public void UpdateManufacturer(UpdateManufacturerRequest updateRequest, Manufacturer existingManufacturer)
        {
            _mapper.Map(updateRequest, existingManufacturer);

            _dbContext.SaveChanges();
            return;
        }

        public List<Manufacturer> GetAllManufacturer()
        {
            return _dbContext.Manufacturers.ToList();
        }

        public Manufacturer? GetManufacturerById(string id)
        {
            return _dbContext.Manufacturers.Where(m => m.Id==id).FirstOrDefault();
        }
    }
}
