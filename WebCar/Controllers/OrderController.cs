using System;
using System.Web.Mvc;
using WebCar.Data;
using WebCar.Models.ViewModels;

namespace WebCar.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly OrderRepository _orderRepo;
        private readonly CarRepository _carRepo;

        public OrderController()
        {
            _orderRepo = new OrderRepository();
            _carRepo = new CarRepository();
        }

        // GET: Order/Create? maXe=1
        [HttpGet]
        public ActionResult Create(int? maXe)
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!maXe.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn xe để đặt hàng";
                return RedirectToAction("Index", "Product");
            }

            // Lấy thông tin xe
            var car = _carRepo.GetCarById(maXe.Value);

            if (car == null)
            {
                TempData["ErrorMessage"] = "Xe không tồn tại";
                return RedirectToAction("Index", "Product");
            }

            var model = new CreateOrderViewModel
            {
                // ✅ SỬA: Convert decimal sang int
                MaXe = (int)car.MAXE,
                TenXe = car.TENXE,
                HangXe = car.HANGXE,
                // ✅ SỬA: Xử lý nullable decimal
                Gia = car.GIA ?? 0,
                HinhAnh = car.HINHANH,
                SoLuong = 1
            };

            return View(model);
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateOrderViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int customerId = (int)Session["CustomerId"];

                var result = _orderRepo.CreateOrder(customerId, model.MaXe, model.SoLuong);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction("Details", new { id = result.OrderId });
                }

                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
                return View(model);
            }
        }

        // GET: Order/MyOrders
        [HttpGet]
        public ActionResult MyOrders()
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int customerId = (int)Session["CustomerId"];
            var orders = _orderRepo.GetMyOrders(customerId);

            ViewBag.CustomerName = Session["CustomerName"];

            return View(orders);
        }

        // GET: Order/Details/5
        [HttpGet]
        public ActionResult Details(int id)
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orderDetail = _orderRepo.GetOrderDetail(id);

            if (orderDetail == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("MyOrders");
            }

            // Kiểm tra quyền: chỉ xem đơn hàng của mình
            int customerId = (int)Session["CustomerId"];
            string userRole = Session["RoleName"]?.ToString() ?? "CUSTOMER";

            if (orderDetail.MaKH != customerId && userRole != "ADMIN" && userRole != "MANAGER")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem đơn hàng này";
                return RedirectToAction("MyOrders");
            }

            return View(orderDetail);
        }

        // POST: Order/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            if (Session["CustomerId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = _orderRepo.UpdateOrderStatus(id, "Da huy");

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Details", new { id = id });
        }
    }
}