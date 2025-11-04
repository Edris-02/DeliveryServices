using DeliveryServices.DataAccess.Data;
using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository
{
    public class DeliveryRouteRepository : Repository<DeliveryRoutes>, IDeliveryRouteRepository
    {
        private readonly ApplicationDbContext _context;
        public DeliveryRouteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(DeliveryRoutes route)
        {
            _context.Update(route);
        }
    }
}