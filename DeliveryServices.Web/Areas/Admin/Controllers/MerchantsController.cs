using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MerchantsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MerchantsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var merchants = _unitOfWork.Merchant.GetAll().ToList();
            return View(merchants);
        }

        public IActionResult Details(int id)
        {
            var merchant = _unitOfWork.Merchant.Get(m => m.Id == id, includeProperties: "Orders.Items,Payouts");
            if (merchant == null)
            {
                return NotFound();
            }
            return View(merchant);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Merchants merchant)
        {
            // Remove validation errors for navigation properties
            ModelState.Remove("Orders");
            ModelState.Remove("Payouts");

            if (ModelState.IsValid)
            {
                merchant.CurrentBalance = 0m;
                merchant.TotalPaidOut = 0m;
                _unitOfWork.Merchant.Add(merchant);
                _unitOfWork.Save();
                TempData["success"] = "Merchant created successfully";
                return RedirectToAction(nameof(Details), new { id = merchant.Id });
            }
            return View(merchant);
        }

        public IActionResult Edit(int id)
        {
            var merchant = _unitOfWork.Merchant.Get(m => m.Id == id);
            if (merchant == null)
            {
                return NotFound();
            }
            return View(merchant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Merchants merchant)
        {
            // Remove validation errors for navigation properties
            ModelState.Remove("Orders");
            ModelState.Remove("Payouts");

            if (ModelState.IsValid)
            {
                _unitOfWork.Merchant.Update(merchant);
                _unitOfWork.Save();
                TempData["success"] = "Merchant updated successfully";
                return RedirectToAction(nameof(Details), new { id = merchant.Id });
            }
            return View(merchant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var merchant = _unitOfWork.Merchant.Get(m => m.Id == id, includeProperties: "Orders");
            if (merchant == null)
            {
                return NotFound();
            }

            if (merchant.Orders.Any())
            {
                TempData["error"] = "Cannot delete merchant with existing orders";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.Merchant.Remove(merchant);
            _unitOfWork.Save();
            TempData["success"] = "Merchant deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
