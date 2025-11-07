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

        // GET: Create a new order
        public IActionResult Create()
        {
            var model = new Orders
            {
                DeliveryFee = 5.00m // Default delivery fee
            };
            
            // Load merchants for dropdown
            ViewBag.Merchants = _unitOfWork.Merchant.GetAll()
                .Select(m => new SelectListItem 
                { 
                    Value = m.Id.ToString(), 
                    Text = m.Name 
                }).ToList();

            // Load active drivers for dropdown
            ViewBag.Drivers = _unitOfWork.Driver.GetAll(d => d.IsActive)
                .Select(d => new SelectListItem 
                { 
                    Value = d.Id.ToString(), 
                    Text = d.FullName 
                }).ToList();

            return View(model);
        }

        // POST: Create a new order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Orders order)
        {
            // Remove validation errors for navigation properties
            ModelState.Remove("Merchant");
            ModelState.Remove("Items");
            ModelState.Remove("Driver");

            if (!ModelState.IsValid)
            {
                // Reload merchants for dropdown
                ViewBag.Merchants = _unitOfWork.Merchant.GetAll()
                    .Select(m => new SelectListItem 
                    { 
                        Value = m.Id.ToString(), 
                        Text = m.Name 
                    }).ToList();

                // Reload drivers for dropdown
                ViewBag.Drivers = _unitOfWork.Driver.GetAll(d => d.IsActive)
                    .Select(d => new SelectListItem 
                    { 
                        Value = d.Id.ToString(), 
                        Text = d.FullName 
                    }).ToList();

                return View(order);
            }

            // Ensure Items collection is initialized
            order.Items ??= new List<OrderItems>();
            order.Merchant = null; // Clear navigation property
            order.Driver = null; // Clear navigation property

            _unitOfWork.Order.Add(order);
            _unitOfWork.Save();

            TempData["success"] = "Order created successfully";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        // GET: Edit order (to update merchant and delivery fee)
        public IActionResult Edit(int id)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == id, includeProperties: "Items,Merchant");
            if (order == null)
            {
                return NotFound();
            }

            // Load merchants for dropdown
            ViewBag.Merchants = _unitOfWork.Merchant.GetAll()
                .Select(m => new SelectListItem 
                { 
                    Value = m.Id.ToString(), 
                    Text = m.Name,
                    Selected = m.Id == order.MerchantId
                }).ToList();

            return View(order);
        }

        // POST: Edit order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Orders order)
        {
            // Remove validation errors for navigation properties
            ModelState.Remove("Merchant");
            ModelState.Remove("Items");

            if (!ModelState.IsValid)
            {
                // Reload merchants for dropdown
                ViewBag.Merchants = _unitOfWork.Merchant.GetAll()
                    .Select(m => new SelectListItem 
                    { 
                        Value = m.Id.ToString(), 
                        Text = m.Name,
                        Selected = m.Id == order.MerchantId
                    }).ToList();
                return View(order);
            }

            // Get the original order to check if merchant changed
            var originalOrder = _unitOfWork.Order.Get(o => o.Id == order.Id, includeProperties: "Items");
            
            // If order is delivered and merchant changed, update balances
            if (order.Status == OrderStatus.Delivered && originalOrder.MerchantId != order.MerchantId)
            {
                // Remove from old merchant balance
                if (originalOrder.MerchantId.HasValue)
                {
                    var oldMerchant = _unitOfWork.Merchant.Get(m => m.Id == originalOrder.MerchantId.Value, tracked: true);
                    if (oldMerchant != null)
                    {
                        oldMerchant.CurrentBalance -= originalOrder.SubTotal;
                        _unitOfWork.Merchant.Update(oldMerchant);
                    }
                }

                // Add to new merchant balance
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

            order.Merchant = null; // Clear navigation property

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

            // When order status is set to Delivered, mark all items as Delivered
            if (status == OrderStatus.Delivered)
            {
                order.DeliveredAt = DateTime.UtcNow;

                // Update all items to Delivered status
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

                // Update merchant balance only if status changed to Delivered
                if (previousStatus != OrderStatus.Delivered && order.MerchantId.HasValue)
                {
                    var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                    if (merchant != null)
                    {
                        merchant.CurrentBalance += order.SubTotal;
                        _unitOfWork.Merchant.Update(merchant);
                    }
                }

                // Update driver stats when order is delivered
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
            // When order status is set to Cancelled, mark all items as Cancelled
            else if (status == OrderStatus.Cancelled)
            {
                // Update all items to Cancelled status
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

                // If was previously delivered, revert merchant balance
                if (previousStatus == OrderStatus.Delivered && order.MerchantId.HasValue)
                {
                    var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                    if (merchant != null)
                    {
                        merchant.CurrentBalance -= order.SubTotal;
                        _unitOfWork.Merchant.Update(merchant);
                    }
                }

                // If was previously delivered, revert driver stats
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
            // For other statuses (Pending, PickedUp, InTransit)
            else
            {
                // If was previously delivered, revert balances and stats
                if (previousStatus == OrderStatus.Delivered)
                {
                    // Revert merchant balance
                    if (order.MerchantId.HasValue)
                    {
                        var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                        if (merchant != null)
                        {
                            merchant.CurrentBalance -= order.SubTotal;
                            _unitOfWork.Merchant.Update(merchant);
                        }
                    }

                    // Revert driver stats
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

        // POST: Add an item to an order
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

            // Store the old subtotal before adding item
            var oldSubTotal = order.SubTotal;

            var item = new OrderItems
            {
                OrderId = orderId,
                ProductName = productName.Trim(),
                ProductSKU = string.IsNullOrWhiteSpace(productSKU) ? null : productSKU.Trim(),
                ProductDescription = string.IsNullOrWhiteSpace(productDescription) ? null : productDescription.Trim(),
                Quantity = quantity,
                UnitPrice = unitPrice,
                Status = OrderItemStatus.Pending // New items start as Pending
            };

            _unitOfWork.OrderItem.Add(item);
            _unitOfWork.Save();

            // No merchant balance update for pending items - only update when item is delivered
            TempData["success"] = "Item added to order";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        // POST: Update an existing order item
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

            // Store old item value if it's delivered
            var oldItemValue = item.Status == OrderItemStatus.Delivered ? (item.Quantity * item.UnitPrice) : 0m;

            // Update item
            item.ProductName = productName.Trim();
            item.ProductSKU = string.IsNullOrWhiteSpace(productSKU) ? null : productSKU.Trim();
            item.ProductDescription = string.IsNullOrWhiteSpace(productDescription) ? null : productDescription.Trim();
            item.Quantity = quantity;
            item.UnitPrice = unitPrice;

            var newItemValue = item.Status == OrderItemStatus.Delivered ? (item.Quantity * item.UnitPrice) : 0m;
            var difference = newItemValue - oldItemValue;

            _unitOfWork.OrderItem.Update(item);
            _unitOfWork.Save();

            // If order is already delivered and item is delivered, update merchant balance
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

        // POST: Remove an item from an order
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

            // Store the item value before removing (only if delivered)
            var itemValue = item.Status == OrderItemStatus.Delivered ? (item.Quantity * item.UnitPrice) : 0m;

            _unitOfWork.OrderItem.Remove(item);
            _unitOfWork.Save();

            // If order is already delivered and item was delivered, update merchant balance
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

        // POST: Update individual item status
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

            // Update merchant balance based on status change
            if (order.Status == OrderStatus.Delivered && order.MerchantId.HasValue)
            {
                var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                if (merchant != null)
                {
                    // If item changed TO delivered, add to merchant balance
                    if (status == OrderItemStatus.Delivered && previousStatus != OrderItemStatus.Delivered)
                    {
                        merchant.CurrentBalance += itemValue;
                    }
                    // If item changed FROM delivered to something else, subtract from merchant balance
                    else if (previousStatus == OrderItemStatus.Delivered && status != OrderItemStatus.Delivered)
                    {
                        merchant.CurrentBalance -= itemValue;
                    }

                    _unitOfWork.Merchant.Update(merchant);
                }
            }

            _unitOfWork.OrderItem.Update(item);
            _unitOfWork.Save();

            // Auto-update order status based on all items status
            var allItems = _unitOfWork.OrderItem.GetAll(i => i.OrderId == orderId).ToList();
     
            if (allItems.Any())
            {
                var allDelivered = allItems.All(i => i.Status == OrderItemStatus.Delivered);
                var allCancelled = allItems.All(i => i.Status == OrderItemStatus.Cancelled);
                var allPending = allItems.All(i => i.Status == OrderItemStatus.Pending);

                // If all items are delivered, mark order as delivered
                if (allDelivered && order.Status != OrderStatus.Delivered)
                {
                    order.Status = OrderStatus.Delivered;
                    order.DeliveredAt = DateTime.UtcNow;

                    // Update merchant balance (add full order subtotal)
                    if (order.MerchantId.HasValue)
                    {
                        var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                        if (merchant != null)
                        {
                            merchant.CurrentBalance += order.SubTotal;
                            _unitOfWork.Merchant.Update(merchant);
                        }
                    }

                    // Update driver stats
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
                // If all items are cancelled, mark order as cancelled
                else if (allCancelled && order.Status != OrderStatus.Cancelled)
                {
                    var wasDelivered = order.Status == OrderStatus.Delivered;
                    order.Status = OrderStatus.Cancelled;

                    // If was delivered, revert merchant balance
                    if (wasDelivered && order.MerchantId.HasValue)
                    {
                        var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                        if (merchant != null)
                        {
                            merchant.CurrentBalance -= order.SubTotal;
                            _unitOfWork.Merchant.Update(merchant);
                        }
                    }

                    // If was delivered, revert driver stats
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
                // If all items are pending and order is not pending
                else if (allPending && order.Status != OrderStatus.Pending)
                {
                    var wasDelivered = order.Status == OrderStatus.Delivered;
                    order.Status = OrderStatus.Pending;
                    order.DeliveredAt = null;

                    // If was delivered, revert merchant balance
                    if (wasDelivered && order.MerchantId.HasValue)
                    {
                        var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                        if (merchant != null)
                        {
                            merchant.CurrentBalance -= order.SubTotal;
                            _unitOfWork.Merchant.Update(merchant);
                        }
                    }

                    // If was delivered, revert driver stats
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
