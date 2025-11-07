using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository.IRepository
{
    public interface IDriverRepository : IRepository<Driver>
    {
 void Update(Driver driver);
    }
}
