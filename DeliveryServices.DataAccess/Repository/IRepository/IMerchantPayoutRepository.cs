using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository.IRepository
{
    public interface IMerchantPayoutRepository : IRepository<MerchantPayouts>
    {
        void Update(MerchantPayouts payout);
    }
}
