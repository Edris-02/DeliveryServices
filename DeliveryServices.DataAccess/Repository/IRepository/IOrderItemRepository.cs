using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository.IRepository
{
    public interface IOrderItemRepository : IRepository<OrderItems>
    {
        void Update(OrderItems orderItem);
    }
}