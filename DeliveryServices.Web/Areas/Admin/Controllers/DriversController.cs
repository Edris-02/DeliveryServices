using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class DriversController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public DriversController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        // GET: Drivers
        public IActionResult Index()
        {
            var drivers = _unitOfWork.Driver.GetAll(includeProperties: "Orders,SalaryPayments").ToList();
            return View(drivers);
        }

        // GET: Drivers/Details/5
        public IActionResult Details(int id)
        {
            var driver = _unitOfWork.Driver.Get(
           d => d.Id == id,
                   includeProperties: "Orders.Items,SalaryPayments,User");

            if (driver == null)
            {
                TempData["error"] = "Driver not found";
                return RedirectToAction(nameof(Index));
            }

            return View(driver);
        }

        // GET: Drivers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Drivers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Driver driver)
        {
            ModelState.Remove("Orders");
            ModelState.Remove("SalaryPayments");
            ModelState.Remove("User");

            if (!ModelState.IsValid)
            {
                return View(driver);
            }

            try
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(driver.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "A user with this email already exists");
                    return View(driver);
                }

                // Create ApplicationUser for the driver
                var user = new ApplicationUser
                {
                    UserName = driver.Email,
                    Email = driver.Email,
                    FullName = driver.FullName,
                    PhoneNumber = driver.PhoneNumber,
                    EmailConfirmed = true
                };

                // Generate default password: FirstName@123
                var defaultPassword = $"{driver.FullName.Split(' ')[0]}@123";
                var result = await _userManager.CreateAsync(user, defaultPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    ModelState.AddModelError("", $"Failed to create user account: {errors}");
                    return View(driver);
                }

                // Assign Driver role
                await _userManager.AddToRoleAsync(user, UserRoles.Driver);

                // Link driver to user
                driver.UserId = user.Id;
                driver.JoinedDate = DateTime.UtcNow;

                // Save driver
                _unitOfWork.Driver.Add(driver);
                _unitOfWork.Save();

                // Update user with DriverId
                user.DriverId = driver.Id;
                await _userManager.UpdateAsync(user);

                TempData["success"] = $"Driver created successfully! Login credentials - Email: {driver.Email}, Password: {defaultPassword}";
                return RedirectToAction(nameof(Details), new { id = driver.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating driver: {ex.Message}");
                return View(driver);
            }
        }

        // GET: Drivers/Edit/5
        public IActionResult Edit(int id)
        {
            var driver = _unitOfWork.Driver.Get(d => d.Id == id);
            if (driver == null)
            {
                TempData["error"] = "Driver not found";
                return RedirectToAction(nameof(Index));
            }

            return View(driver);
        }

        // POST: Drivers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Driver driver)
        {
            ModelState.Remove("Orders");
            ModelState.Remove("SalaryPayments");
            ModelState.Remove("User");

            if (!ModelState.IsValid)
            {
                return View(driver);
            }

            _unitOfWork.Driver.Update(driver);
            _unitOfWork.Save();

            TempData["success"] = "Driver updated successfully";
            return RedirectToAction(nameof(Details), new { id = driver.Id });
        }

        // POST: Drivers/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleStatus(int id)
        {
            var driver = _unitOfWork.Driver.Get(d => d.Id == id, tracked: true);
            if (driver == null)
            {
                TempData["error"] = "Driver not found";
                return RedirectToAction(nameof(Index));
            }

            driver.IsActive = !driver.IsActive;
            _unitOfWork.Save();

            TempData["success"] = $"Driver {(driver.IsActive ? "activated" : "deactivated")} successfully";
            return RedirectToAction(nameof(Details), new { id = driver.Id });
        }

        // GET: Drivers/PaySalary/5
        public IActionResult PaySalary(int id)
        {
            var driver = _unitOfWork.Driver.Get(
                 d => d.Id == id,
                         includeProperties: "Orders,SalaryPayments");

            if (driver == null)
            {
                TempData["error"] = "Driver not found";
                return RedirectToAction(nameof(Index));
            }

            if (driver.CurrentBalance <= 0)
            {
                TempData["error"] = "Driver has no pending salary";
                return RedirectToAction(nameof(Details), new { id });
            }

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var payment = new DriverSalaryPayment
            {
                DriverId = driver.Id,
                Amount = driver.CurrentBalance,
                BaseSalaryPortion = driver.BaseSalary > 0 ? driver.BaseSalary / 12 : 0,
                CommissionPortion = driver.CurrentMonthDeliveries * driver.CommissionPerDelivery,
                DeliveriesCount = driver.CurrentMonthDeliveries,
                PaymentDate = now,
                PeriodStart = startOfMonth,
                PeriodEnd = now
            };

            ViewBag.Driver = driver;
            return View(payment);
        }

        // POST: Drivers/PaySalary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PaySalary(DriverSalaryPayment payment)
        {
            ModelState.Remove("Driver");

            var driver = _unitOfWork.Driver.Get(d => d.Id == payment.DriverId, tracked: true);
            if (driver == null)
            {
                TempData["error"] = "Driver not found";
                return RedirectToAction(nameof(Index));
            }

            if (payment.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Amount must be greater than zero");
            }

            if (payment.Amount > driver.CurrentBalance)
            {
                ModelState.AddModelError("Amount", $"Amount cannot exceed current balance of {driver.CurrentBalance:N2}");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Driver = driver;
                return View(payment);
            }

            // Process payment
            payment.ProcessedBy = User.Identity?.Name ?? "Admin";
            _unitOfWork.DriverSalaryPayment.Add(payment);

            // Update driver balance
            driver.CurrentBalance -= payment.Amount;
            driver.TotalEarnings += payment.Amount;
            driver.CurrentMonthDeliveries = 0; // Reset monthly counter

            _unitOfWork.Save();

            TempData["success"] = $"Salary payment of {payment.Amount:N2} processed successfully";
            return RedirectToAction(nameof(Details), new { id = driver.Id });
        }
    }
}
