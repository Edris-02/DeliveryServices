using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository.IRepository
{
    public interface IMerchantRepository : IRepository<Merchants>
    {
        void Update(Merchants merchants);
    }
}
