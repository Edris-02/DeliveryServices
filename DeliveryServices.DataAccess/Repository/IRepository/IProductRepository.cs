using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
    }
}