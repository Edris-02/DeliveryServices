using DeliveryServices.DataAccess.Data;
using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            _context.Update(product);
        }
    }
}