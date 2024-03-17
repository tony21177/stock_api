using stock_api.Models;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Service
{
    public class AuthLayerService
    {
        private readonly StockDbContext _dbContext;

        public AuthLayerService(StockDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<WarehouseAuthlayer> GetAllAuthlayers(string compId)
        {
            return _dbContext.WarehouseAuthlayers.Where(al=>al.CompId==compId).ToList();
        }
        public WarehouseAuthlayer? GetByAuthValue(short authValue, string compId)
        {
            return _dbContext.WarehouseAuthlayers.Where(authLayer => authLayer.AuthValue == authValue&&authLayer.CompId==compId).First();
        }

        public List<WarehouseAuthlayer> UpdateAuthlayers(List<WarehouseAuthlayer> authlayers)
        {
            var updatedAuthLayers = new List<WarehouseAuthlayer>();
            authlayers.ForEach(authlayer =>
            {
                var existingAuthLayer = _dbContext.WarehouseAuthlayers.Find(authlayer.AuthId);
                if (existingAuthLayer != null)
                {
                    // 使用 SetValues 來只更新不為 null 的屬性
                    _dbContext.Entry(existingAuthLayer).CurrentValues.SetValues(authlayer);
                    updatedAuthLayers.Add(existingAuthLayer);
                }
            });
            // 將變更保存到資料庫
            _dbContext.SaveChanges();
            return updatedAuthLayers;
        }

        public WarehouseAuthlayer AddAuthlayer(WarehouseAuthlayer newAuthlayer)
        {
            _dbContext.WarehouseAuthlayers.Add(newAuthlayer);
            _dbContext.SaveChanges(true);
            return newAuthlayer;
        }

        public void DeleteAuthLayer(int id)
        {
            var authLayerToDelete = new WarehouseAuthlayer { AuthId = id };
            // 將實體的狀態設置為 'Deleted'
            _dbContext.Entry(authLayerToDelete).State = EntityState.Deleted;

            // 將更改應用到資料庫
            _dbContext.SaveChanges();
            return;
        }
    }
}
