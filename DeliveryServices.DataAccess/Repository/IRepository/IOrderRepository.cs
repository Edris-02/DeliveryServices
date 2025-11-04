using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository.IRepository
{
    public interface IOrderRepository : IRepository<Orders>
    {
        void Update(Orders order);
    }
}