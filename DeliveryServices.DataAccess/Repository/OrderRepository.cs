using DeliveryServices.DataAccess.Data;
using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository
{
    public class OrderRepository : Repository<Orders>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Orders order)
        {
            _context.Update(order);
        }
    }
}