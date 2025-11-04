using DeliveryServices.DataAccess.Data;
using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository
{
    public class OrderItemRepository : Repository<OrderItems>, IOrderItemRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderItemRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderItems orderItem)
        {
            _context.Update(orderItem);
        }
    }
}