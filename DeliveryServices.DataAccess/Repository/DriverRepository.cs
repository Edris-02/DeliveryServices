using DeliveryServices.DataAccess.Data;
using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository
{
    public class DriverRepository : Repository<Driver>, IDriverRepository
    {
      private readonly ApplicationDbContext _context;

public DriverRepository(ApplicationDbContext context) : base(context)
   {
            _context = context;
     }

        public void Update(Driver driver)
  {
            _context.Update(driver);
   }
    }
}
