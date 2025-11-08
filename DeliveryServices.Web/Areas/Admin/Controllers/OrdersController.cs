using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public class OrdersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var orders = _unitOfWork.Order.GetAll(includeProperties: "Items,Merchant").ToList();
            return View(orders);
        }

        public IActionResult Details(int id)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == id, includeProperties: "Items,Merchant");
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        public IActionResult Create()
        {
            var model = new Orders
            {
                DeliveryFee = 5.00m
            };

            ViewBag.Merchants = _unitOfWork.Merchant.GetAll()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Name
                }).ToList();

            ViewBag.Drivers = _unitOfWork.Driver.GetAll(d => d.IsActive)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.FullName
                }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Orders order)
        {
            ModelState.Remove("Merchant");
            ModelState.Remove("Items");
            ModelState.Remove("Driver");

            if (!ModelState.IsValid)
            {
                ViewBag.Merchants = _unitOfWork.Merchant.GetAll()
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Name
                    }).ToList();

                ViewBag.Drivers = _unitOfWork.Driver.GetAll(d => d.IsActive)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.FullName
                    }).ToList();

                return View(order);
            }

            order.Items ??= new List<OrderItems>();
            order.Merchant = null;
            order.Driver = null;

            _unitOfWork.Order.Add(order);
            _unitOfWork.Save();

            TempData["success"] = "Order created successfully";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        public IActionResult Edit(int id)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == id, includeProperties: "Items,Merchant");
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Merchants = _unitOfWork.Merchant.GetAll()
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Name,
                    Selected = m.Id == order.MerchantId
                }).ToList();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Orders order)
        {
            ModelState.Remove("Merchant");
            ModelState.Remove("Items");

            if (!ModelState.IsValid)
            {
                ViewBag.Merchants = _unitOfWork.Merchant.GetAll()
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Name,
                        Selected = m.Id == order.MerchantId
                    }).ToList();
                return View(order);
            }

            var originalOrder = _unitOfWork.Order.Get(o => o.Id == order.Id, includeProperties: "Items");

            if (order.Status == OrderStatus.Delivered && originalOrder.MerchantId != order.MerchantId)
            {
                if (originalOrder.MerchantId.HasValue)
                {
                    var oldMerchant = _unitOfWork.Merchant.Get(m => m.Id == originalOrder.MerchantId.Value, tracked: true);
                    if (oldMerchant != null)
                    {
                        oldMerchant.CurrentBalance -= originalOrder.SubTotal;
                        _unitOfWork.Merchant.Update(oldMerchant);
                    }
                }

                if (order.MerchantId.HasValue)
                {
                    var newMerchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                    if (newMerchant != null)
                    {
                        newMerchant.CurrentBalance += order.SubTotal;
                        _unitOfWork.Merchant.Update(newMerchant);
                    }
                }
            }

            order.Merchant = null;

            _unitOfWork.Order.Update(order);
            _unitOfWork.Save();

            TempData["success"] = "Order updated successfully";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, OrderStatus status)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == id, includeProperties: "Items,Merchant,Driver", tracked: true);
            if (order == null)
            {
                return NotFound();
            }

            var previousStatus = order.Status;
            order.Status = status;

            if (status == OrderStatus.Delivered)
            {
                order.DeliveredAt = DateTime.UtcNow;

                foreach (var item in order.Items)
                {
                    if (item.Status != OrderItemStatus.Delivered)
                    {
                        var trackedItem = _unitOfWork.OrderItem.Get(i => i.Id == item.Id, tracked: true);
                        if (trackedItem != null)
                        {
                            trackedItem.Status = OrderItemStatus.Delivered;
                            _unitOfWork.OrderItem.Update(trackedItem);
                        }
                    }
                }

                if (previousStatus != OrderStatus.Delivered && order.MerchantId.HasValue)
                {
                    var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                    if (merchant != null)
                    {
                        merchant.CurrentBalance += order.SubTotal;
                        _unitOfWork.Merchant.Update(merchant);
                    }
                }

                if (previousStatus != OrderStatus.Delivered && order.DriverId.HasValue)
                {
                    var driver = _unitOfWork.Driver.Get(d => d.Id == order.DriverId.Value, tracked: true);
                    if (driver != null)
                    {
                        driver.TotalDeliveries++;
                        driver.CurrentMonthDeliveries++;
                        driver.CurrentBalance += driver.CommissionPerDelivery;
                        _unitOfWork.Driver.Update(driver);
                    }
                }
            }
            else if (status == OrderStatus.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    if (item.Status != OrderItemStatus.Cancelled)
                    {
                        var trackedItem = _unitOfWork.OrderItem.Get(i => i.Id == item.Id, tracked: true);
                        if (trackedItem != null)
                        {
                            trackedItem.Status = OrderItemStatus.Cancelled;
                            _unitOfWork.OrderItem.Update(trackedItem);
                        }
                    }
                }

                if (previousStatus == OrderStatus.Delivered && order.MerchantId.HasValue)
                {
                    var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                    if (merchant != null)
                    {
                        merchant.CurrentBalance -= order.SubTotal;
                        _unitOfWork.Merchant.Update(merchant);
                    }
                }

                if (previousStatus == OrderStatus.Delivered && order.DriverId.HasValue)
                {
                    var driver = _unitOfWork.Driver.Get(d => d.Id == order.DriverId.Value, tracked: true);
                    if (driver != null && driver.TotalDeliveries > 0)
                    {
                        driver.TotalDeliveries--;
                        if (driver.CurrentMonthDeliveries > 0)
                            driver.CurrentMonthDeliveries--;
                        driver.CurrentBalance -= driver.CommissionPerDelivery;
                        _unitOfWork.Driver.Update(driver);
                    }
                }
            }
            else
            {
                if (previousStatus == OrderStatus.Delivered)
                {
                    if (order.MerchantId.HasValue)
                    {
                        var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                        if (merchant != null)
                        {
                            merchant.CurrentBalance -= order.SubTotal;
                            _unitOfWork.Merchant.Update(merchant);
                        }
                    }

                    if (order.DriverId.HasValue)
                    {
                        var driver = _unitOfWork.Driver.Get(d => d.Id == order.DriverId.Value, tracked: true);
                        if (driver != null && driver.TotalDeliveries > 0)
                        {
                            driver.TotalDeliveries--;
                            if (driver.CurrentMonthDeliveries > 0)
                                driver.CurrentMonthDeliveries--;
                            driver.CurrentBalance -= driver.CommissionPerDelivery;
                            _unitOfWork.Driver.Update(driver);
                        }
                    }
                }
            }

            _unitOfWork.Order.Update(order);
            _unitOfWork.Save();

            TempData["success"] = "Order status updated successfully";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddItem(
            int orderId,
            string productName,
            string? productSKU,
            string? productDescription,
            int quantity,
            decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(productName) || quantity <= 0 || unitPrice < 0)
            {
                TempData["error"] = "Invalid item details";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            var order = _unitOfWork.Order.Get(o => o.Id == orderId, includeProperties: "Items", tracked: true);
            if (order == null)
            {
                return NotFound();
            }

            var oldSubTotal = order.SubTotal;

            var item = new OrderItems
            {
                OrderId = orderId,
                ProductName = productName.Trim(),
                ProductSKU = string.IsNullOrWhiteSpace(productSKU) ? null : productSKU.Trim(),
                ProductDescription = string.IsNullOrWhiteSpace(productDescription) ? null : productDescription.Trim(),
                Quantity = quantity,
                UnitPrice = unitPrice,
                Status = OrderItemStatus.Pending
            };

            _unitOfWork.OrderItem.Add(item);
            _unitOfWork.Save();

            TempData["success"] = "Item added to order";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateItem(
            int orderId,
            int itemId,
            string productName,
            string? productSKU,
            string? productDescription,
            int quantity,
            decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(productName) || quantity <= 0 || unitPrice < 0)
            {
                TempData["error"] = "Invalid item details";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            var order = _unitOfWork.Order.Get(o => o.Id == orderId, includeProperties: "Items");
            if (order == null)
            {
                return NotFound();
            }

            var item = _unitOfWork.OrderItem.Get(oi => oi.Id == itemId && oi.OrderId == orderId, tracked: true);
            if (item == null)
            {
                return NotFound();
            }

            var oldItemValue = item.Status == OrderItemStatus.Delivered ? (item.Quantity * item.UnitPrice) : 0m;

            item.ProductName = productName.Trim();
            item.ProductSKU = string.IsNullOrWhiteSpace(productSKU) ? null : productSKU.Trim();
            item.ProductDescription = string.IsNullOrWhiteSpace(productDescription) ? null : productDescription.Trim();
            item.Quantity = quantity;
            item.UnitPrice = unitPrice;

            var newItemValue = item.Status == OrderItemStatus.Delivered ? (item.Quantity * item.UnitPrice) : 0m;
            var difference = newItemValue - oldItemValue;

            _unitOfWork.OrderItem.Update(item);
            _unitOfWork.Save();

            if (order.Status == OrderStatus.Delivered && order.MerchantId.HasValue && difference != 0)
            {
                var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                if (merchant != null)
                {
                    merchant.CurrentBalance += difference;
                    _unitOfWork.Merchant.Update(merchant);
                    _unitOfWork.Save();
                }
            }

            TempData["success"] = "Item updated";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(int orderId, int itemId)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == orderId, includeProperties: "Items");
            if (order == null)
            {
                return NotFound();
            }

            var item = _unitOfWork.OrderItem.Get(oi => oi.Id == itemId && oi.OrderId == orderId);
            if (item == null)
            {
                return NotFound();
            }

            var itemValue = item.Status == OrderItemStatus.Delivered ? (item.Quantity * item.UnitPrice) : 0m;

            _unitOfWork.OrderItem.Remove(item);
            _unitOfWork.Save();

            if (order.Status == OrderStatus.Delivered && order.MerchantId.HasValue && itemValue > 0)
            {
                var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                if (merchant != null)
                {
                    merchant.CurrentBalance -= itemValue;
                    _unitOfWork.Merchant.Update(merchant);
                    _unitOfWork.Save();
                }
            }

            TempData["success"] = "Item removed from order";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateItemStatus(int orderId, int itemId, OrderItemStatus status)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == orderId, includeProperties: "Items,Merchant", tracked: true);
            if (order == null)
            {
                return NotFound();
            }

            var item = _unitOfWork.OrderItem.Get(oi => oi.Id == itemId && oi.OrderId == orderId, tracked: true);
            if (item == null)
            {
                return NotFound();
            }

            var previousStatus = item.Status;
            item.Status = status;

            var itemValue = item.Quantity * item.UnitPrice;

            if (order.Status == OrderStatus.Delivered && order.MerchantId.HasValue)
            {
                var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                if (merchant != null)
                {
                    if (status == OrderItemStatus.Delivered && previousStatus != OrderItemStatus.Delivered)
                    {
                        merchant.CurrentBalance += itemValue;
                    }
                    else if (previousStatus == OrderItemStatus.Delivered && status != OrderItemStatus.Delivered)
                    {
                        merchant.CurrentBalance -= itemValue;
                    }

                    _unitOfWork.Merchant.Update(merchant);
                }
            }

            _unitOfWork.OrderItem.Update(item);
            _unitOfWork.Save();

            var allItems = _unitOfWork.OrderItem.GetAll(i => i.OrderId == orderId).ToList();

            if (allItems.Any())
            {
                var allDelivered = allItems.All(i => i.Status == OrderItemStatus.Delivered);
                var allCancelled = allItems.All(i => i.Status == OrderItemStatus.Cancelled);
                var allPending = allItems.All(i => i.Status == OrderItemStatus.Pending);

                if (allDelivered && order.Status != OrderStatus.Delivered)
                {
                    order.Status = OrderStatus.Delivered;
                    order.DeliveredAt = DateTime.UtcNow;

                    if (order.MerchantId.HasValue)
                    {
                        var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                        if (merchant != null)
                        {
                            merchant.CurrentBalance += order.SubTotal;
                            _unitOfWork.Merchant.Update(merchant);
                        }
                    }

                    if (order.DriverId.HasValue)
                    {
                        var driver = _unitOfWork.Driver.Get(d => d.Id == order.DriverId.Value, tracked: true);
                        if (driver != null)
                        {
                            driver.TotalDeliveries++;
                            driver.CurrentMonthDeliveries++;
                            driver.CurrentBalance += driver.CommissionPerDelivery;
                            _unitOfWork.Driver.Update(driver);
                        }
                    }

                    _unitOfWork.Order.Update(order);
                    _unitOfWork.Save();

                    TempData["success"] = "All items delivered! Order marked as Delivered";
                    return RedirectToAction(nameof(Details), new { id = orderId });
                }
                else if (allCancelled && order.Status != OrderStatus.Cancelled)
                {
                    var wasDelivered = order.Status == OrderStatus.Delivered;
                    order.Status = OrderStatus.Cancelled;

                    if (wasDelivered && order.MerchantId.HasValue)
                    {
                        var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                        if (merchant != null)
                        {
                            merchant.CurrentBalance -= order.SubTotal;
                            _unitOfWork.Merchant.Update(merchant);
                        }
                    }

                    if (wasDelivered && order.DriverId.HasValue)
                    {
                        var driver = _unitOfWork.Driver.Get(d => d.Id == order.DriverId.Value, tracked: true);
                        if (driver != null && driver.TotalDeliveries > 0)
                        {
                            driver.TotalDeliveries--;
                            if (driver.CurrentMonthDeliveries > 0)
                                driver.CurrentMonthDeliveries--;
                            driver.CurrentBalance -= driver.CommissionPerDelivery;
                            _unitOfWork.Driver.Update(driver);
                        }
                    }

                    _unitOfWork.Order.Update(order);
                    _unitOfWork.Save();

                    TempData["success"] = "All items cancelled! Order marked as Cancelled";
                    return RedirectToAction(nameof(Details), new { id = orderId });
                }
                else if (allPending && order.Status != OrderStatus.Pending)
                {
                    var wasDelivered = order.Status == OrderStatus.Delivered;
                    order.Status = OrderStatus.Pending;
                    order.DeliveredAt = null;

                    if (wasDelivered && order.MerchantId.HasValue)
                    {
                        var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                        if (merchant != null)
                        {
                            merchant.CurrentBalance -= order.SubTotal;
                            _unitOfWork.Merchant.Update(merchant);
                        }
                    }

                    if (wasDelivered && order.DriverId.HasValue)
                    {
                        var driver = _unitOfWork.Driver.Get(d => d.Id == order.DriverId.Value, tracked: true);
                        if (driver != null && driver.TotalDeliveries > 0)
                        {
                            driver.TotalDeliveries--;
                            if (driver.CurrentMonthDeliveries > 0)
                                driver.CurrentMonthDeliveries--;
                            driver.CurrentBalance -= driver.CommissionPerDelivery;
                            _unitOfWork.Driver.Update(driver);
                        }
                    }

                    _unitOfWork.Order.Update(order);
                    _unitOfWork.Save();

                    TempData["success"] = "All items pending! Order marked as Pending";
                    return RedirectToAction(nameof(Details), new { id = orderId });
                }
            }

            TempData["success"] = $"Item status updated to {status}";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}
