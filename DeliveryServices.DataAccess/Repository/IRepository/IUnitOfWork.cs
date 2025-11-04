using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryServices.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IMerchantRepository Merchants { get; }
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }
        IDeliveryRouteRepository DeliveryRoutes { get; }
        void SaveChanges();
    }
}
