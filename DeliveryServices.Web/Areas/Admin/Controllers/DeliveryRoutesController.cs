using DeliveryServices.DataAccess.Repository.IRepository;
using DeliveryServices.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace DeliveryServices.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DeliveryRoutesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeliveryRoutesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var routes = _unitOfWork.DeliveryRoute.GetAll(includeProperties: "Orders").ToList();
            return View(routes);
        }

        public IActionResult Create()
        {
            return View(new DeliveryRoutes { ScheduledAt = DateTime.UtcNow.AddDays(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DeliveryRoutes route)
        {
            // Remove validation errors for navigation property
            ModelState.Remove("Orders");

            if (ModelState.IsValid)
            {
                route.Status = RouteStatus.Planned;
                _unitOfWork.DeliveryRoute.Add(route);
                _unitOfWork.Save();
                TempData["success"] = "Route created successfully";
                return RedirectToAction(nameof(Details), new { id = route.Id });
            }
            return View(route);
        }

        public IActionResult Edit(int id)
        {
            var route = _unitOfWork.DeliveryRoute.Get(r => r.Id == id, includeProperties: "Orders");
            if (route == null) return NotFound();
            return View(route);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DeliveryRoutes route)
        {
            // Remove validation errors for navigation property
            ModelState.Remove("Orders");

            if (ModelState.IsValid)
            {
                _unitOfWork.DeliveryRoute.Update(route);
                _unitOfWork.Save();
                TempData["success"] = "Route updated successfully";
                return RedirectToAction(nameof(Details), new { id = route.Id });
            }
            return View(route);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var route = _unitOfWork.DeliveryRoute.Get(r => r.Id == id, includeProperties: "Orders");
            if (route == null) return NotFound();

            // Unassign all orders from this route before deleting
            if (route.Orders.Any())
            {
                foreach (var order in route.Orders.ToList())
                {
                    order.DeliveryRouteId = null;
                    _unitOfWork.Order.Update(order);
                }
            }

            _unitOfWork.DeliveryRoute.Remove(route);
            _unitOfWork.Save();
            TempData["success"] = "Route deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var route = _unitOfWork.DeliveryRoute.Get(r => r.Id == id, includeProperties: "Orders.Items");
            if (route == null) return NotFound();
            
            // Get unassigned orders for the assignment dropdown
            ViewBag.UnassignedOrders = _unitOfWork.Order
                .GetAll(o => o.DeliveryRouteId == null && o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
                .ToList();
            
            return View(route);
        }

        // Assign an order to this route
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignOrder(int routeId, int orderId)
        {
            var route = _unitOfWork.DeliveryRoute.Get(r => r.Id == routeId);
            var order = _unitOfWork.Order.Get(o => o.Id == orderId, tracked: true);

            if (route == null || order == null)
            {
                return NotFound();
            }

            if (order.DeliveryRouteId != null)
            {
                TempData["error"] = "Order is already assigned to another route";
                return RedirectToAction(nameof(Details), new { id = routeId });
            }

            order.DeliveryRouteId = routeId;
            _unitOfWork.Order.Update(order);
            _unitOfWork.Save();

            TempData["success"] = $"Order #{orderId} assigned to route";
            return RedirectToAction(nameof(Details), new { id = routeId });
        }

        // Unassign an order from this route
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UnassignOrder(int routeId, int orderId)
        {
            var order = _unitOfWork.Order.Get(o => o.Id == orderId && o.DeliveryRouteId == routeId, tracked: true);
            
            if (order == null)
            {
                return NotFound();
            }

            order.DeliveryRouteId = null;
            _unitOfWork.Order.Update(order);
            _unitOfWork.Save();

            TempData["success"] = $"Order #{orderId} unassigned from route";
            return RedirectToAction(nameof(Details), new { id = routeId });
        }

        // Start the route (courier begins delivery)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartRoute(int id)
        {
            var route = _unitOfWork.DeliveryRoute.Get(r => r.Id == id, includeProperties: "Orders", tracked: true);
            
            if (route == null) return NotFound();

            if (route.Status != RouteStatus.Planned)
            {
                TempData["error"] = "Only planned routes can be started";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (!route.Orders.Any())
            {
                TempData["error"] = "Cannot start a route with no orders";
                return RedirectToAction(nameof(Details), new { id });
            }

            route.Status = RouteStatus.InProgress;
            route.StartedAt = DateTime.UtcNow;

            // Update all orders to InTransit
            foreach (var order in route.Orders.Where(o => o.Status == OrderStatus.Pending))
            {
                order.Status = OrderStatus.InTransit;
                _unitOfWork.Order.Update(order);
            }

            _unitOfWork.DeliveryRoute.Update(route);
            _unitOfWork.Save();

            TempData["success"] = "Route started successfully";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Complete the route (all deliveries finished)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteRoute(int id)
        {
            var route = _unitOfWork.DeliveryRoute.Get(r => r.Id == id, includeProperties: "Orders", tracked: true);
            
            if (route == null) return NotFound();

            if (route.Status != RouteStatus.InProgress)
            {
                TempData["error"] = "Only in-progress routes can be completed";
                return RedirectToAction(nameof(Details), new { id });
            }

            route.Status = RouteStatus.Completed;
            route.CompletedAt = DateTime.UtcNow;

            _unitOfWork.DeliveryRoute.Update(route);
            _unitOfWork.Save();

            TempData["success"] = "Route completed successfully";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Cancel the route
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelRoute(int id)
        {
            var route = _unitOfWork.DeliveryRoute.Get(r => r.Id == id, tracked: true);
            
            if (route == null) return NotFound();

            if (route.Status == RouteStatus.Completed)
            {
                TempData["error"] = "Cannot cancel a completed route";
                return RedirectToAction(nameof(Details), new { id });
            }

            route.Status = RouteStatus.Cancelled;

            _unitOfWork.DeliveryRoute.Update(route);
            _unitOfWork.Save();

            TempData["success"] = "Route cancelled";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
