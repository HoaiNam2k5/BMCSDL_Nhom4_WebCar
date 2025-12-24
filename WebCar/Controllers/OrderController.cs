using System;
using System.Web.Mvc;
using WebCar.Data;
using WebCar.Models.ViewModels;
using WebCar.Helpers;
using System.Collections.Generic;

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

            var car = _carRepo.GetCarById(maXe.Value);

            if (car == null)
            {
                TempData["ErrorMessage"] = "Xe không tồn tại";
                return RedirectToAction("Index", "Product");
            }

            // Get customer info for pre-fill
            int customerId = (int)Session["CustomerId"];
            var customer = GetCustomerInfo(customerId);

            var model = new CreateOrderViewModel
            {
                MaXe = (int)car.MAXE,
                TenXe = car.TENXE,
                HangXe = car.HANGXE,
                Gia = car.GIA ?? 0,
                HinhAnh = car.HINHANH,
                SoLuong = 1,
                // Pre-fill customer info
                DiaChiGiaoHang = customer?.DiaChi ?? "",
                SoDienThoai = customer?.SDT ?? ""
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
                    var car = _carRepo.GetCarById(model.MaXe);
                    if (car != null)
                    {
                        model.TenXe = car.TENXE;
                        model.HangXe = car.HANGXE;
                        model.Gia = car.GIA ?? 0;
                        model.HinhAnh = car.HINHANH;
                    }
                    return View(model);
                }

                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int customerId = (int)Session["CustomerId"];

                // ✅ RSA ENCRYPTION WITH TRY-CATCH
                string encryptedAddress = "";
                string encryptedPhone = "";

                System.Diagnostics.Debug.WriteLine("========================================");
                System.Diagnostics.Debug.WriteLine("RSA ENCRYPTION - ORDER CREATE");
                System.Diagnostics.Debug.WriteLine($"Customer ID:  {customerId}");
                System.Diagnostics.Debug.WriteLine($"Car ID: {model.MaXe}");
                System.Diagnostics.Debug.WriteLine($"Quantity: {model.SoLuong}");

                // Encrypt Address
                if (!string.IsNullOrEmpty(model.DiaChiGiaoHang))
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Original Address: {model.DiaChiGiaoHang}");
                        encryptedAddress = RSAEncryptionHelper.Encrypt(model.DiaChiGiaoHang);
                        System.Diagnostics.Debug.WriteLine($"✅ Encrypted Address Length: {encryptedAddress.Length}");
                    }
                    catch (Exception rsaEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ RSA Address Encryption Failed: {rsaEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"   Stack: {rsaEx.StackTrace}");

                        ModelState.AddModelError("", "Lỗi mã hóa địa chỉ: " + rsaEx.Message);

                        // Reload car info
                        var car = _carRepo.GetCarById(model.MaXe);
                        if (car != null)
                        {
                            model.TenXe = car.TENXE;
                            model.HangXe = car.HANGXE;
                            model.Gia = car.GIA ?? 0;
                            model.HinhAnh = car.HINHANH;
                        }

                        return View(model);
                    }
                }

                // Encrypt Phone
                if (!string.IsNullOrEmpty(model.SoDienThoai))
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Original Phone: {model.SoDienThoai}");
                        encryptedPhone = RSAEncryptionHelper.Encrypt(model.SoDienThoai);
                        System.Diagnostics.Debug.WriteLine($"✅ Encrypted Phone Length: {encryptedPhone.Length}");
                    }
                    catch (Exception rsaEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ RSA Phone Encryption Failed: {rsaEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"   Stack: {rsaEx.StackTrace}");

                        ModelState.AddModelError("", "Lỗi mã hóa số điện thoại: " + rsaEx.Message);

                        // Reload car info
                        var car = _carRepo.GetCarById(model.MaXe);
                        if (car != null)
                        {
                            model.TenXe = car.TENXE;
                            model.HangXe = car.HANGXE;
                            model.Gia = car.GIA ?? 0;
                            model.HinhAnh = car.HINHANH;
                        }

                        return View(model);
                    }
                }

                System.Diagnostics.Debug.WriteLine("========================================");

                // Create order with encrypted data
                var result = _orderRepo.CreateOrder(
                    customerId,
                    model.MaXe,
                    model.SoLuong,
                    encryptedAddress,
                    encryptedPhone,
                    model.GhiChu
                );

                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"Đặt hàng thành công!  Mã đơn hàng: #{result.OrderId}";
                    TempData["NewOrderId"] = result.OrderId;

                    LogOrderAction(customerId, $"CREATE_ORDER_ENCRYPTED:  #{result.OrderId}");

                    return RedirectToAction("MyOrders");
                }

                ModelState.AddModelError("", result.Message);

                // Reload car info
                var carReload = _carRepo.GetCarById(model.MaXe);
                if (carReload != null)
                {
                    model.TenXe = carReload.TENXE;
                    model.HangXe = carReload.HANGXE;
                    model.Gia = carReload.GIA ?? 0;
                    model.HinhAnh = carReload.HINHANH;
                }

                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Order Create Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                ModelState.AddModelError("", "Lỗi:  " + ex.Message);

                // Reload car info
                var car = _carRepo.GetCarById(model.MaXe);
                if (car != null)
                {
                    model.TenXe = car.TENXE;
                    model.HangXe = car.HANGXE;
                    model.Gia = car.GIA ?? 0;
                    model.HinhAnh = car.HINHANH;
                }

                return View(model);
            }
        }
        // =========================================
        // GET: Order/MyOrders
        // =========================================
        [HttpGet]
        public ActionResult MyOrders()
        {
            try
            {
                // ✅ CHECK SESSION
                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int customerId = (int)Session["CustomerId"];
                string roleName = Session["RoleName"]?.ToString() ?? "CUSTOMER";

                System.Diagnostics.Debug.WriteLine("========================================");
                System.Diagnostics.Debug.WriteLine($"MyOrders - Customer:  {customerId}, Role: {roleName}");

                // ✅ GET ORDERS (will be filtered by role in repository)
                var orders = _orderRepo.GetMyOrders(customerId, roleName);

                System.Diagnostics.Debug.WriteLine($"Orders retrieved: {orders.Count}");
                System.Diagnostics.Debug.WriteLine("========================================");

                return View(orders);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ MyOrders Error: {ex.Message}");
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách đơn hàng:  " + ex.Message;
                return View(new List<OrderViewModel>());
            }
        }

        // =========================================
        // GET:  Order/Details/5
        // =========================================
        [HttpGet]
        public ActionResult Details(int id)
        {
            try
            {
                // ✅ CHECK SESSION
                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int customerId = (int)Session["CustomerId"];
                string roleName = Session["RoleName"]?.ToString() ?? "CUSTOMER";

                System.Diagnostics.Debug.WriteLine("========================================");
                System.Diagnostics.Debug.WriteLine($"Order Details - Order:  {id}, Customer: {customerId}, Role: {roleName}");

                // ✅ GET ORDER DETAILS
                var order = _orderRepo.GetOrderDetails(id);

                if (order == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Order #{id} not found");
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng! ";
                    return RedirectToAction("MyOrders");
                }

                // ✅ CHECK PERMISSION
                bool hasPermission = false;

                if (roleName == "ADMIN")
                {
                    // Admin sees all
                    hasPermission = true;
                    System.Diagnostics.Debug.WriteLine("✅ Admin - Access granted");
                }
                else if (roleName == "MANAGER")
                {
                    // Manager sees non-cancelled orders
                    hasPermission = order.TrangThai != "Da huy";
                    System.Diagnostics.Debug.WriteLine($"Manager - Access:  {hasPermission} (Status: {order.TrangThai})");
                }
                else
                {
                    // Customer sees only own orders
                    hasPermission = (order.MaKH == customerId);
                    System.Diagnostics.Debug.WriteLine($"Customer - Order owner: {order.MaKH}, Current user: {customerId}, Access: {hasPermission}");
                }

                if (!hasPermission)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ACCESS DENIED");
                    System.Diagnostics.Debug.WriteLine("========================================");

                    TempData["ErrorMessage"] = "Bạn không có quyền xem đơn hàng này!";
                    return RedirectToAction("MyOrders");
                }

                System.Diagnostics.Debug.WriteLine($"✅ ACCESS GRANTED");
                System.Diagnostics.Debug.WriteLine("========================================");

                return View(order);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Order Details Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine("========================================");

                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết đơn hàng: " + ex.Message;
                return RedirectToAction("MyOrders");
            }
        }

        // =========================================
        // POST: Order/Cancel/5
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            try
            {
                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int customerId = (int)Session["CustomerId"];
                string roleName = Session["RoleName"]?.ToString() ?? "CUSTOMER";

                System.Diagnostics.Debug.WriteLine($"Cancel Order #{id} - User: {customerId}, Role: {roleName}");

                // ✅ Check permission first
                var order = _orderRepo.GetOrderDetails(id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                    return RedirectToAction("MyOrders");
                }

                // Only owner or admin can cancel
                if (order.MaKH != customerId && roleName != "ADMIN")
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền hủy đơn hàng này!";
                    return RedirectToAction("MyOrders");
                }

                // Cancel order
                var result = _orderRepo.CancelOrder(id, customerId);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction("MyOrders");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Cancel Order Error: {ex.Message}");
                TempData["ErrorMessage"] = "Lỗi khi hủy đơn hàng: " + ex.Message;
                return RedirectToAction("MyOrders");
            }
        }
    
    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Get customer information
    /// </summary>
    private dynamic GetCustomerInfo(int customerId)
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                        SELECT MAKH, HOTEN, EMAIL, SDT, DIACHI
                        FROM CUSTOMER
                        WHERE MAKH = :customerId", conn);

                    cmd.Parameters.Add("customerId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = customerId;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new
                            {
                                MaKH = Convert.ToInt32(reader["MAKH"]),
                                HoTen = reader["HOTEN"]?.ToString(),
                                Email = reader["EMAIL"]?.ToString(),
                                SDT = reader["SDT"]?.ToString(),
                                DiaChi = reader["DIACHI"]?.ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ GetCustomerInfo Error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Log order actions to AUDIT_LOG
        /// </summary>
        private void LogOrderAction(int customerId, string action)
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                        INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
                        VALUES (SEQ_LOG.NEXTVAL, :matk, :hanhdong, 'ORDER', SYSDATE, :ip)", conn);

                    cmd.Parameters.Add("matk", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = customerId;
                    cmd.Parameters.Add("hanhdong", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2).Value = action;
                    cmd.Parameters.Add("ip", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2).Value =
                        Request.UserHostAddress ?? "Unknown";

                    cmd.ExecuteNonQuery();

                    System.Diagnostics.Debug.WriteLine($"✅ Logged:  {action}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ LogOrderAction Error: {ex.Message}");
                // Don't throw - logging failure shouldn't break the flow
            }
        }
    }
}