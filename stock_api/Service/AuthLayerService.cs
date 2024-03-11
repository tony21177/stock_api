using stock_api.Models;
using Microsoft.EntityFrameworkCore;

namespace stock_api.Service
{
    public class AuthLayerService
    {
        private readonly HandoverContext _dbContext;

        public AuthLayerService(HandoverContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Authlayer> GetAllAuthlayers()
        {
            return _dbContext.Authlayers.ToList();
        }
        public Authlayer? GetByAuthValue(short authValue)
        {
            return _dbContext.Authlayers.Where(authLayer => authLayer.AuthValue == authValue).First();
        }

        public List<Authlayer> UpdateAuthlayers(List<Authlayer> authlayers)
        {
            var updatedAuthLayers = new List<Authlayer>();
            authlayers.ForEach(authlayer =>
            {
                var existingAuthLayer = _dbContext.Authlayers.Find(authlayer.Id);
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

        public Authlayer AddAuthlayer(Authlayer newAuthlayer)
        {
            _dbContext.Authlayers.Add(newAuthlayer);
            _dbContext.SaveChanges(true);
            return newAuthlayer;
        }

        public void DeleteAuthLayer(int id)
        {
            var authLayerToDelete = new Authlayer { Id = id };
            // 將實體的狀態設置為 'Deleted'
            _dbContext.Entry(authLayerToDelete).State = EntityState.Deleted;

            // 將更改應用到資料庫
            _dbContext.SaveChanges();
            return;
        }
    }
}
