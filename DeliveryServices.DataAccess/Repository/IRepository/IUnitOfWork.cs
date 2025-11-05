using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryServices.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IMerchantRepository Merchant { get; }
        IOrderItemRepository OrderItem { get; }
        IOrderRepository Order { get; }
        IDeliveryRouteRepository DeliveryRoute { get; }
        IMerchantPayoutRepository MerchantPayout { get; }
        void Save();
    }
}
