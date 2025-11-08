using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class SearchController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                TempData["info"] = "Please enter a search term";
                return RedirectToAction("Index", "Dashboard");
            }

            var searchTerm = q.Trim().ToLower();

            var results = new SearchResults
            {
                Query = q,
                Orders = SearchOrders(searchTerm),
                Merchants = SearchMerchants(searchTerm),
                Drivers = SearchDrivers(searchTerm)
            };

            return View(results);
        }

        private List<Orders> SearchOrders(string searchTerm)
        {
            return _unitOfWork.Order.GetAll(includeProperties: "Merchant,Driver")
         .Where(o =>
                    o.Id.ToString().Contains(searchTerm) ||
            o.CustomerName.ToLower().Contains(searchTerm) ||
            o.CustomerPhone.ToLower().Contains(searchTerm) ||
             o.CustomerAddress.ToLower().Contains(searchTerm) ||
                    o.Status.ToString().ToLower().Contains(searchTerm) ||
             (o.Merchant != null && o.Merchant.Name.ToLower().Contains(searchTerm)) ||
           (o.Driver != null && o.Driver.FullName.ToLower().Contains(searchTerm))
            )
              .OrderByDescending(o => o.CreatedAt)
                      .Take(10)
          .ToList();
        }

        private List<Merchants> SearchMerchants(string searchTerm)
        {
            return _unitOfWork.Merchant.GetAll()
              .Where(m =>
                 m.Name.ToLower().Contains(searchTerm) ||
                     m.Email.ToLower().Contains(searchTerm) ||
            m.PhoneNumber.ToLower().Contains(searchTerm) ||
                 m.Address.ToLower().Contains(searchTerm)
                       )
                          .Take(10)
                  .ToList();
        }

        private List<Models.Driver> SearchDrivers(string searchTerm)
        {
            return _unitOfWork.Driver.GetAll()
           .Where(d =>
                 d.FullName.ToLower().Contains(searchTerm) ||
                         d.Email.ToLower().Contains(searchTerm) ||
               d.PhoneNumber.ToLower().Contains(searchTerm) ||
                     (d.LicenseNumber != null && d.LicenseNumber.ToLower().Contains(searchTerm)) ||
               (d.VehiclePlateNumber != null && d.VehiclePlateNumber.ToLower().Contains(searchTerm))
                           )
               .Take(10)
                           .ToList();
        }
    }

    public class SearchResults
    {
        public string Query { get; set; } = string.Empty;
        public List<Orders> Orders { get; set; } = new();
        public List<Merchants> Merchants { get; set; } = new();
        public List<Models.Driver> Drivers { get; set; } = new();

        public int TotalResults => Orders.Count + Merchants.Count + Drivers.Count;
    }
}
