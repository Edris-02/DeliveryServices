using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class MerchantPayoutsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MerchantPayoutsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var payouts = _unitOfWork.MerchantPayout.GetAll(includeProperties: "Merchant").ToList();
            return View(payouts);
        }

        public IActionResult Details(int id)
        {
            var payout = _unitOfWork.MerchantPayout.Get(p => p.Id == id, includeProperties: "Merchant");
            if (payout == null)
            {
                return NotFound();
            }
            return View(payout);
        }

        public IActionResult Create(int merchantId)
        {
            var merchant = _unitOfWork.Merchant.Get(m => m.Id == merchantId, includeProperties: "Orders,Payouts");
            if (merchant == null)
            {
                return NotFound();
            }

            if (merchant.CurrentBalance <= 0)
            {
                TempData["error"] = "Merchant has no balance to pay out";
                return RedirectToAction("Details", "Merchants", new { id = merchantId });
            }

            var model = new MerchantPayouts
            {
                MerchantId = merchantId,
                Amount = merchant.CurrentBalance,
                PaidAt = DateTime.UtcNow
            };

            ViewBag.Merchant = merchant;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MerchantPayouts payout)
        {
            ModelState.Remove("Merchant");

            var merchant = _unitOfWork.Merchant.Get(m => m.Id == payout.MerchantId, tracked: true);
            if (merchant == null)
            {
                return NotFound();
            }

            if (payout.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Amount must be greater than zero");
            }

            if (payout.Amount > merchant.CurrentBalance)
            {
                ModelState.AddModelError("Amount", $"Amount cannot exceed current balance of ${merchant.CurrentBalance:N2}");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Merchant = merchant;
                return View(payout);
            }

            merchant.CurrentBalance -= payout.Amount;
            merchant.TotalPaidOut += payout.Amount;

            payout.PaidAt = DateTime.UtcNow;
            payout.Merchant = null; 
            _unitOfWork.MerchantPayout.Add(payout);
            _unitOfWork.Merchant.Update(merchant);
            _unitOfWork.Save();

            TempData["success"] = $"Payment of ${payout.Amount:N2} processed successfully";
            return RedirectToAction("Details", "Merchants", new { id = payout.MerchantId });
        }

        public IActionResult MerchantHistory(int merchantId)
        {
            var merchant = _unitOfWork.Merchant.Get(m => m.Id == merchantId, includeProperties: "Payouts");
            if (merchant == null)
            {
                return NotFound();
            }

            ViewBag.Merchant = merchant;
            return View(merchant.Payouts.OrderByDescending(p => p.PaidAt).ToList());
        }
    }
}
