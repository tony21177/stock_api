﻿using AutoMapper;
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

        public void AddCompany(Company company)
        {
            _dbContext.Companies.Add(company);
            _dbContext.SaveChanges();
            return;
        }

        public CompanyWithUnitVo? GetCompanyWithUnit(string compId)
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


        public void AddCompanyUnit(CompanyUnit companyUnit)
        {
            _dbContext.CompanyUnits.Add(companyUnit);
            _dbContext.SaveChanges();
            return ;
        }


    }
}
