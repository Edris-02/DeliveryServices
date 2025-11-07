using DeliveryServices.Models;

namespace DeliveryServices.DataAccess.Repository.IRepository
{
    public interface IDriverSalaryPaymentRepository : IRepository<DriverSalaryPayment>
    {
        void Update(DriverSalaryPayment payment);
    }
}
