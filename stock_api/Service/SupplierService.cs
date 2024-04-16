using AutoMapper;
using stock_api.Controllers.Request;
using stock_api.Models;

namespace stock_api.Service
{
    public class SupplierService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public SupplierService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public void AddSupplier(Supplier supplier)
        {
            _dbContext.Suppliers.Add(supplier);
            _dbContext.SaveChanges();
        }

        public void UpdateSupplier(UpdateSupplierRequest updateRequest, Supplier existingSupplier)
        {
            _mapper.Map(updateRequest, existingSupplier);

            _dbContext.SaveChanges();
            return;
        }

        public List<Supplier> GetAllSupplier()
        {
            return _dbContext.Suppliers.ToList();
        }

        public List<Supplier> GetAllSupplierByCompId(string compId)
        {
            return _dbContext.Suppliers.Where(s=>s.CompId==compId).ToList();
        }

        public Supplier? GetSupplierById(int id)
        {
            return _dbContext.Suppliers.Where(s => s.Id==id).FirstOrDefault();
        }

        public List<Supplier> GetSuppliersByIdList(List<int> supplierIdList)
        {
            return _dbContext.Suppliers.Where(s=>supplierIdList.Contains(s.Id)).ToList();
        }
    }
}
