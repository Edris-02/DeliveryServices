using DeliveryServices.DataAccess.Data;
using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository
{
    public class DriverSalaryPaymentRepository : Repository<DriverSalaryPayment>, IDriverSalaryPaymentRepository
    {
   private readonly ApplicationDbContext _context;

        public DriverSalaryPaymentRepository(ApplicationDbContext context) : base(context)
{
          _context = context;
    }

  public void Update(DriverSalaryPayment payment)
{
  _context.Update(payment);
  }
    }
}
