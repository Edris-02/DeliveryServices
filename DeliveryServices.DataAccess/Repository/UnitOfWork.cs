using DeliveryServices.DataAccess.Data;
using DeliveryServices.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryServices.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Merchant = new MerchantRepository(_db);
            OrderItem = new OrderItemRepository(_db);
            Order = new OrderRepository(_db);
            MerchantPayout = new MerchantPayoutRepository(_db);
            Driver = new DriverRepository(_db);
            DriverSalaryPayment = new DriverSalaryPaymentRepository(_db);
        }

        public IMerchantRepository Merchant { get; private set; }
        public IOrderItemRepository OrderItem { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IMerchantPayoutRepository MerchantPayout { get; private set; }
        public IDriverRepository Driver { get; private set; }
        public IDriverSalaryPaymentRepository DriverSalaryPayment { get; private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
