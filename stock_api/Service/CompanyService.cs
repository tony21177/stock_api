using AutoMapper;
using stock_api.Common.Constant;
using stock_api.Controllers.Request;
using stock_api.Models;
using stock_api.Service.ValueObject;
using System.ComponentModel.Design;

namespace stock_api.Service
{
    public class CompanyService
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public CompanyService(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }


        public List<CompanyWithUnitVo> GetAllCompanyWithUnitByUnitId(string unitId)
        {
            var query = from c in _dbContext.Companies
                        join cu in _dbContext.CompanyUnits on c.CompId equals cu.CompId
                        where cu.UnitId == unitId
                        select new CompanyWithUnitVo
                        {
                            CompId = c.CompId,
                            Name = c.Name,
                            IsActive = c.IsActive,
                            Type = c.Type,
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt,
                            UnitId = cu.UnitId,
                            UnitName = cu.UnitName
                        };

            var result = query.ToList();
            
            return result;
        }

        public List<CompanyWithUnitVo> GetAllCompanyWithUnit()
        {
            var query = from c in _dbContext.Companies
                        join cu in _dbContext.CompanyUnits on c.CompId equals cu.CompId
                        select new CompanyWithUnitVo
                        {
                            CompId = c.CompId,
                            Name = c.Name,
                            IsActive = c.IsActive,
                            Type = c.Type,
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt,
                            UnitId = cu.UnitId,
                            UnitName = cu.UnitName
                        };

            var result = query.ToList();

            return result;
        }

        public Company? GetCompanyByCompId(string compId)
        {
            return _dbContext.Companies.Where(cp => cp.CompId == compId).FirstOrDefault();
        }
        public List<Company> GetCompanyByCompIds(List<string> compIds)
        {
            return _dbContext.Companies.Where(cp => compIds.Contains(cp.CompId)).ToList();
        }

        public CompanyWithUnitVo? GetCompanyWithUnitByCompanyId(string compId)
        {
            var query = from c in _dbContext.Companies
                        join cu in _dbContext.CompanyUnits on c.CompId equals cu.CompId
                        where c.CompId == compId
                        select new CompanyWithUnitVo
                        {
                            CompId = c.CompId,
                            Name = c.Name,
                            IsActive = c.IsActive,
                            Type = c.Type,
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt,
                            UnitId = cu.UnitId,
                            UnitName = cu.UnitName
                        };

            var result = query.ToList();
            if (result.Count > 0)
            {
                return result[0];
            }
            return null;
        }

        public List<CompanyWithUnitVo> GetCompanyWithUnitListByCompanyIdList(List<string> compIdList)
        {
            var query = from c in _dbContext.Companies
                        join cu in _dbContext.CompanyUnits on c.CompId equals cu.CompId
                        where compIdList.Contains(c.CompId)
                        select new CompanyWithUnitVo
                        {
                            CompId = c.CompId,
                            Name = c.Name,
                            IsActive = c.IsActive,
                            Type = c.Type,
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt,
                            UnitId = cu.UnitId,
                            UnitName = cu.UnitName
                        };

            var result = query.ToList();
            
            return result;
        }

        public void AddCompany(Company company,String unitId,String unitName)
        {
            company.CompId = Guid.NewGuid().ToString();
            company.Type = CommonConstants.CompanyType.ORGANIZATION;
            _dbContext.Companies.Add(company);
            var companyUnit = new CompanyUnit()
            {
                CompId = company.CompId,
                CompName = company.Name,
                UnitId = unitId,
                UnitName = unitName
            };
            _dbContext.CompanyUnits.Add(companyUnit);
            _dbContext.SaveChanges();
            return;
        }


        public void AddCompanyUnit(CompanyUnit companyUnit)
        {
            _dbContext.CompanyUnits.Add(companyUnit);
            _dbContext.SaveChanges();
            return ;
        }

        public void UpdateCompany(UpdateCompanyRequest updateCompanyRequest,Company existingCompany)
        {
            _mapper.Map(updateCompanyRequest, existingCompany);
            _dbContext.SaveChanges();
            return;
        }

    }
}
