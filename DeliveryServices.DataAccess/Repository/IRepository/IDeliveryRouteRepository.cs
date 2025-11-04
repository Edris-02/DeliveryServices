using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository.IRepository
{
    public interface IDeliveryRouteRepository : IRepository<DeliveryRoutes>
    {
        void Update(DeliveryRoutes route);
    }
}