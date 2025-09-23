using DeliveryServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryServices.DataAccess.Repository.IRepository
{
    public interface IMerchantRepository : IRepository<Merchants>
    {
        void Update(Merchants merchants);
    }
}
