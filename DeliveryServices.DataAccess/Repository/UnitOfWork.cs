using DeliveryServices.DataAccess.Data;
using DeliveryServices.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryServices.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IMerchantRepository Merchants { get; private set; }
        public IProductRepository Products { get; private set; }
        public IOrderRepository Orders { get; private set; }
        public IOrderItemRepository OrderItems { get; private set; }
        public IDeliveryRouteRepository DeliveryRoutes { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Merchants = new MerchantRepository(_context);
            Products = new ProductRepository(_context);
            Orders = new OrderRepository(_context);
            OrderItems = new OrderItemRepository(_context);
            DeliveryRoutes = new DeliveryRouteRepository(_context);
        }
        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
