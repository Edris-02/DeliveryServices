using System;
using System.Collections.Generic;
using System.Linq;
using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
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

            if (!ModelState.IsValid)
            {
                // Reload merchants for dropdown
                ViewBag.Merchants = _unitOfWork.Merchant.GetAll()
                    .Select(m => new SelectListItem 
                    { 
                        Value = m.Id.ToString(), 
                        Text = m.Name 
                    }).ToList();
                return View(order);
            }

            // Ensure Items collection is initialized
            order.Items ??= new List<OrderItems>();
            order.Merchant = null; // Clear navigation property

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
            var order = _unitOfWork.Order.Get(o => o.Id == id, includeProperties: "Items,Merchant", tracked: true);
            if (order == null)
            {
                return NotFound();
            }

            var previousStatus = order.Status;
            order.Status = status;

            if (status == OrderStatus.Delivered && previousStatus != OrderStatus.Delivered)
            {
                order.DeliveredAt = DateTime.UtcNow;

                // Update merchant balance when order is delivered
                if (order.MerchantId.HasValue)
                {
                    var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                    if (merchant != null)
                    {
                        // Add the merchant's share (subtotal, excluding delivery fee) to their balance
                        merchant.CurrentBalance += order.SubTotal;
                        _unitOfWork.Merchant.Update(merchant);
                    }
                }
            }
            else if (previousStatus == OrderStatus.Delivered && status != OrderStatus.Delivered)
            {
                // If order was delivered but now changed to another status, subtract from merchant balance
                if (order.MerchantId.HasValue)
                {
                    var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                    if (merchant != null)
                    {
                        merchant.CurrentBalance -= order.SubTotal;
                        _unitOfWork.Merchant.Update(merchant);
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
                UnitPrice = unitPrice
            };

            _unitOfWork.OrderItem.Add(item);
            _unitOfWork.Save();

            // If order is already delivered, update merchant balance
            if (order.Status == OrderStatus.Delivered && order.MerchantId.HasValue)
            {
                // Reload order to get updated SubTotal with new item
                var updatedOrder = _unitOfWork.Order.Get(o => o.Id == orderId, includeProperties: "Items");
                var newSubTotal = updatedOrder.SubTotal;
                var difference = newSubTotal - oldSubTotal;

                var merchant = _unitOfWork.Merchant.Get(m => m.Id == order.MerchantId.Value, tracked: true);
                if (merchant != null)
                {
                    merchant.CurrentBalance += difference;
                    _unitOfWork.Merchant.Update(merchant);
                    _unitOfWork.Save();
                }
            }

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

            // Store old item value
            var oldItemValue = item.Quantity * item.UnitPrice;

            // Update item
            item.ProductName = productName.Trim();
            item.ProductSKU = string.IsNullOrWhiteSpace(productSKU) ? null : productSKU.Trim();
            item.ProductDescription = string.IsNullOrWhiteSpace(productDescription) ? null : productDescription.Trim();
            item.Quantity = quantity;
            item.UnitPrice = unitPrice;

            var newItemValue = item.Quantity * item.UnitPrice;
            var difference = newItemValue - oldItemValue;

            _unitOfWork.OrderItem.Update(item);
            _unitOfWork.Save();

            // If order is already delivered, update merchant balance
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

            // Store the item value before removing
            var itemValue = item.Quantity * item.UnitPrice;

            _unitOfWork.OrderItem.Remove(item);
            _unitOfWork.Save();

            // If order is already delivered, update merchant balance
            if (order.Status == OrderStatus.Delivered && order.MerchantId.HasValue)
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
    }
}
