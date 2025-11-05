using DeliveryServices.DataAccess.Data;
using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository
{
    public class MerchantPayoutRepository : Repository<MerchantPayouts>, IMerchantPayoutRepository
    {
        private readonly ApplicationDbContext _context;

        public MerchantPayoutRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(MerchantPayouts payout)
        {
            _context.Update(payout);
        }
    }
}
