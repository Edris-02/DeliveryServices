using DeliveryServices.DataAccess.Data;
using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryServices.DataAccess.Repository
{
    public class MerchantRepository : Repository<Merchants>, IMerchantRepository
    {
        private readonly ApplicationDbContext _context;
        public MerchantRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public void Update(Merchants merchants)
        {
            _context.Update(merchants);
        }
    }
}
