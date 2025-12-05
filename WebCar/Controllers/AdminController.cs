using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebCar.Data;
using WebCar.Filters;
using WebCar.Models;
using WebCar.Models.ViewModels;

namespace WebCar.Controllers
{
    [AuthorizeRole("ADMIN")]
    public class AdminController : Controller
    {
        private readonly CustomerRepository _customerRepo;
        private readonly CarRepository _carRepo;
        private readonly OrderRepository _orderRepo;

        public AdminController()
        {
            _customerRepo = new CustomerRepository();
            _carRepo = new CarRepository();
            _orderRepo = new OrderRepository();
        }

        // ==================== DASHBOARD ====================

        // GET: Admin/Index
        public ActionResult Index()
        {
            try
            {
                ViewBag.TotalCustomers = GetTotalCustomers();
                ViewBag.TotalCars = GetTotalCars();
                ViewBag.TotalOrders = GetTotalOrders();
                ViewBag.TotalRevenue = GetTotalRevenue();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View();
            }
        }

        // ==================== USERS MANAGEMENT ====================

        // GET: Admin/Users
        public ActionResult Users()
        {
            try
            {
                var users = GetAllUsersWithDetails();
                return View(users);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View(new List<UserManagementViewModel>());
            }
        }

        // GET: Admin/GetUserDetail
        [HttpGet]
        public JsonResult GetUserDetail(int id)
        {
            try
            {
                var user = GetUserDetailById(id);

                if (user != null)
                {
                    return Json(new { success = true, data = user }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = "Không tìm thấy user" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/ChangeUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangeUserRole(int userId, string newRole)
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                        UPDATE ACCOUNT_ROLE
                        SET ROLENAME = :newRole
                        WHERE MATK = :userId", conn);

                    cmd.Parameters.Add("newRole", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2).Value = newRole;
                    cmd.Parameters.Add("userId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = userId;

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        LogAdminAction($"CHANGE_ROLE: User #{userId} to {newRole}");
                        return Json(new { success = true, message = "Đổi role thành công!" });
                    }

                    return Json(new { success = false, message = "Không thể đổi role!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteUser(int userId)
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    // Xóa role
                    var deleteRoleCmd = new Oracle.ManagedDataAccess.Client.OracleCommand(
                        "DELETE FROM ACCOUNT_ROLE WHERE MATK = :userId", conn);
                    deleteRoleCmd.Parameters.Add("userId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = userId;
                    deleteRoleCmd.ExecuteNonQuery();

                    // Xóa customer
                    var deleteUserCmd = new Oracle.ManagedDataAccess.Client.OracleCommand(
                        "DELETE FROM CUSTOMER WHERE MAKH = :userId", conn);
                    deleteUserCmd.Parameters.Add("userId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = userId;
                    int rowsAffected = deleteUserCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        LogAdminAction($"DELETE_USER: User #{userId}");
                        return Json(new { success = true, message = "Xóa user thành công!" });
                    }

                    return Json(new { success = false, message = "Không thể xóa user!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ==================== PRODUCTS MANAGEMENT ====================

        // GET: Admin/Products
        public ActionResult Products()
        {
            try
            {
                var cars = _carRepo.GetAllCars();
                return View(cars);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View(new List<CAR>());
            }
        }

        // GET: Admin/GetProductDetail
        [HttpGet]
        public JsonResult GetProductDetail(int id)
        {
            try
            {
                var car = _carRepo.GetCarById(id);

                if (car != null)
                {
                    return Json(new
                    {
                        success = true,
                        data = new
                        {
                            maxe = car.MAXE,
                            tenxe = car.TENXE,
                            hangxe = car.HANGXE,
                            gia = car.GIA,
                            namsx = car.NAMSX,
                            hinhanh = car.HINHANH,
                            trangthai = car.TRANGTHAI,
                            mota = car.MOTA
                        }
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = "Không tìm thấy sản phẩm" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateProduct(CAR model)
        {
            try
            {
                var result = _carRepo.CreateCar(model);

                if (result.Success)
                {
                    LogAdminAction($"CREATE_PRODUCT: {model.TENXE}");
                    TempData["SuccessMessage"] = result.Message;
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/UpdateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateProduct(CAR model)
        {
            try
            {
                var result = _carRepo.UpdateCar(model);

                if (result.Success)
                {
                    LogAdminAction($"UPDATE_PRODUCT: #{model.MAXE} - {model.TENXE}");
                    TempData["SuccessMessage"] = result.Message;
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/UpdateProductStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateProductStatus(int productId, string newStatus)
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                        UPDATE CAR
                        SET TRANGTHAI = :newStatus
                        WHERE MAXE = :productId", conn);

                    cmd.Parameters.Add("newStatus", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2).Value = newStatus;
                    cmd.Parameters.Add("productId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = productId;

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        LogAdminAction($"UPDATE_STATUS: Product #{productId} to {newStatus}");
                        return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
                    }

                    return Json(new { success = false, message = "Không thể cập nhật!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Admin/DeleteProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteProduct(int productId)
        {
            try
            {
                var result = _carRepo.DeleteCar(productId);

                if (result.Success)
                {
                    LogAdminAction($"DELETE_PRODUCT: #{productId}");
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ==================== ORDERS MANAGEMENT ====================

        // GET: Admin/Orders
        public ActionResult Orders()
        {
            try
            {
                var orders = GetAllOrdersWithDetails();
                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View(new List<AdminOrderViewModel>());
            }
        }

        // POST: Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                var result = _orderRepo.UpdateOrderStatus(orderId, newStatus);

                if (result.Success)
                {
                    LogAdminAction($"UPDATE_ORDER_STATUS: Order #{orderId} to {newStatus}");
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // ==================== SECURITY ====================

        // GET: Admin/Security
        public ActionResult Security()
        {
            try
            {
                ViewBag.TotalLogs = GetTotalAuditLogs();
                ViewBag.FailedLogins = GetFailedLoginAttempts();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View();
            }
        }
        // GET: Account/AccessDenied
        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View();
        }

        // POST: Account/LogAccessDenied
        [HttpPost]
        public JsonResult LogAccessDenied(string url)
        {
            try
            {
                if (Session["CustomerId"] != null)
                {
                    int userId = (int)Session["CustomerId"];

                    using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                        System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                    {
                        conn.Open();

                        var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                    INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
                    VALUES (SEQ_LOG.NEXTVAL, :matk, :hanhdong, 'ACCESS_DENIED', SYSDATE, :ip)", conn);

                        cmd.Parameters.Add("matk", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = userId;
                        cmd.Parameters.Add("hanhdong", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2).Value =
                            $"ACCESS_DENIED: {url}";
                        cmd.Parameters.Add("ip", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2).Value =
                            Request.UserHostAddress;

                        cmd.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
        // ==================== HELPER METHODS ====================

        private int GetTotalCustomers()
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();
                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(
                        "SELECT COUNT(*) FROM CUSTOMER", conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 0; }
        }

        private int GetTotalCars()
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();
                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(
                        "SELECT COUNT(*) FROM CAR WHERE TRANGTHAI != 'Da xoa'", conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 0; }
        }

        private int GetTotalOrders()
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();
                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(
                        "SELECT COUNT(*) FROM ORDERS", conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 0; }
        }

        private decimal GetTotalRevenue()
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();
                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(
                        "SELECT NVL(SUM(TONGTIEN), 0) FROM ORDERS WHERE TRANGTHAI != 'Da huy'", conn);
                    return Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            catch { return 0; }
        }

        private int GetTotalAuditLogs()
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();
                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(
                        "SELECT COUNT(*) FROM AUDIT_LOG", conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 0; }
        }

        private int GetFailedLoginAttempts()
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();
                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(
                        "SELECT COUNT(*) FROM AUDIT_LOG WHERE HANHDONG LIKE 'ACCESS_DENIED%' AND NGAYGIO >= SYSDATE - 30", conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch { return 0; }
        }

        private List<UserManagementViewModel> GetAllUsersWithDetails()
        {
            var users = new List<UserManagementViewModel>();

            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                        SELECT 
                            c.MAKH,
                            c.HOTEN,
                            c.EMAIL,
                            c.SDT,
                            c.DIACHI,
                            c.NGAYDANGKY,
                            NVL(ar.ROLENAME, 'CUSTOMER') AS ROLENAME,
                            (SELECT COUNT(*) FROM ORDERS WHERE MAKH = c.MAKH) AS TOTAL_ORDERS,
                            (SELECT COUNT(*) FROM AUDIT_LOG WHERE MATK = c.MAKH) AS TOTAL_ACTIVITIES
                        FROM CUSTOMER c
                        LEFT JOIN ACCOUNT_ROLE ar ON c.MAKH = ar.MATK
                        ORDER BY c.MAKH DESC", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new UserManagementViewModel
                            {
                                MaKH = Convert.ToInt32(reader["MAKH"]),
                                HoTen = reader["HOTEN"]?.ToString() ?? "",
                                Email = reader["EMAIL"]?.ToString() ?? "",
                                SDT = reader["SDT"]?.ToString() ?? "",
                                DiaChi = reader["DIACHI"]?.ToString() ?? "",
                                NgayDangKy = Convert.ToDateTime(reader["NGAYDANGKY"]),
                                RoleName = reader["ROLENAME"]?.ToString() ?? "CUSTOMER",
                                TotalOrders = Convert.ToInt32(reader["TOTAL_ORDERS"]),
                                TotalActivities = Convert.ToInt32(reader["TOTAL_ACTIVITIES"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetAllUsersWithDetails: {ex.Message}");
            }

            return users;
        }

        private dynamic GetUserDetailById(int userId)
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                        SELECT 
                            c.MAKH,
                            c.HOTEN,
                            c.EMAIL,
                            c.SDT,
                            c.DIACHI,
                            TO_CHAR(c.NGAYDANGKY, 'DD/MM/YYYY HH24:MI') AS NGAYDANGKY,
                            NVL(ar.ROLENAME, 'CUSTOMER') AS ROLENAME,
                            (SELECT COUNT(*) FROM ORDERS WHERE MAKH = c.MAKH) AS TOTAL_ORDERS
                        FROM CUSTOMER c
                        LEFT JOIN ACCOUNT_ROLE ar ON c.MAKH = ar.MATK
                        WHERE c.MAKH = :userId", conn);

                    cmd.Parameters.Add("userId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = userId;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new
                            {
                                maKH = Convert.ToInt32(reader["MAKH"]),
                                hoTen = reader["HOTEN"]?.ToString(),
                                email = reader["EMAIL"]?.ToString(),
                                sdt = reader["SDT"]?.ToString(),
                                diaChi = reader["DIACHI"]?.ToString(),
                                ngayDangKy = reader["NGAYDANGKY"]?.ToString(),
                                roleName = reader["ROLENAME"]?.ToString(),
                                totalOrders = Convert.ToInt32(reader["TOTAL_ORDERS"])
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetUserDetailById: {ex.Message}");
            }

            return null;
        }

        private List<AdminOrderViewModel> GetAllOrdersWithDetails()
        {
            var orders = new List<AdminOrderViewModel>();

            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                        SELECT 
                            o.MADON,
                            o.MAKH,
                            c.HOTEN,
                            c.EMAIL,
                            c.SDT,
                            o.NGAYDAT,
                            o.TONGTIEN,
                            o.TRANGTHAI,
                            od.MAXE,
                            car.TENXE,
                            car.HANGXE,
                            od.SOLUONG,
                            od.DONGIA
                        FROM ORDERS o
                        JOIN CUSTOMER c ON o. MAKH = c.MAKH
                        JOIN ORDER_DETAIL od ON o.MADON = od.MADON
                        JOIN CAR car ON od. MAXE = car.MAXE
                        ORDER BY o.NGAYDAT DESC", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new AdminOrderViewModel
                            {
                                MaDon = Convert.ToInt32(reader["MADON"]),
                                MaKH = Convert.ToInt32(reader["MAKH"]),
                                HoTen = reader["HOTEN"]?.ToString() ?? "",
                                Email = reader["EMAIL"]?.ToString() ?? "",
                                SDT = reader["SDT"]?.ToString() ?? "",
                                NgayDat = Convert.ToDateTime(reader["NGAYDAT"]),
                                TongTien = Convert.ToDecimal(reader["TONGTIEN"]),
                                TrangThai = reader["TRANGTHAI"]?.ToString() ?? "",
                                MaXe = Convert.ToInt32(reader["MAXE"]),
                                TenXe = reader["TENXE"]?.ToString() ?? "",
                                HangXe = reader["HANGXE"]?.ToString() ?? "",
                                SoLuong = Convert.ToInt32(reader["SOLUONG"]),
                                DonGia = Convert.ToDecimal(reader["DONGIA"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetAllOrdersWithDetails: {ex.Message}");
            }

            return orders;
        }

        // ✅ ĐÚNG VỊ TRÍ: Helper method log admin actions
        private void LogAdminAction(string action)
        {
            try
            {
                if (Session["CustomerId"] == null) return;

                int adminId = (int)Session["CustomerId"];

                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                        INSERT INTO AUDIT_LOG (MALOG, MATK, HANHDONG, BANGTACDONG, NGAYGIO, IP)
                        VALUES (SEQ_LOG.NEXTVAL, :matk, :hanhdong, 'ADMIN_ACTION', SYSDATE, '127.0.0.1')", conn);

                    cmd.Parameters.Add("matk", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = adminId;
                    cmd.Parameters.Add("hanhdong", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2).Value = action;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error LogAdminAction: {ex.Message}");
            }
        }
        // ==================== SECURITY & AUDIT LOGS API ====================

        // GET: Admin/GetAuditLogs
        [HttpGet]
        public JsonResult GetAuditLogs(int page = 1, int pageSize = 50)
        {
            try
            {
                var logs = new List<dynamic>();

                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                SELECT * FROM (
                    SELECT 
                        al.MALOG,
                        al.MATK,
                        al.HANHDONG,
                        al.BANGTACDONG,
                        al.NGAYGIO,
                        al.IP,
                        c.HOTEN,
                        c.EMAIL,
                        ROW_NUMBER() OVER (ORDER BY al.NGAYGIO DESC) AS RN
                    FROM AUDIT_LOG al
                    LEFT JOIN CUSTOMER c ON al.MATK = c.MAKH
                )
                WHERE RN BETWEEN :startRow AND :endRow
                ORDER BY NGAYGIO DESC", conn);

                    int startRow = (page - 1) * pageSize + 1;
                    int endRow = page * pageSize;

                    cmd.Parameters.Add("startRow", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = startRow;
                    cmd.Parameters.Add("endRow", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = endRow;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new
                            {
                                malog = Convert.ToInt32(reader["MALOG"]),
                                matk = reader["MATK"] != DBNull.Value ? Convert.ToInt32(reader["MATK"]) : (int?)null,
                                hanhdong = reader["HANHDONG"]?.ToString() ?? "",
                                bangtacdong = reader["BANGTACDONG"]?.ToString() ?? "",
                                ngaygio = Convert.ToDateTime(reader["NGAYGIO"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                ip = reader["IP"]?.ToString() ?? "",
                                hoten = reader["HOTEN"]?.ToString() ?? "",
                                email = reader["EMAIL"]?.ToString() ?? ""
                            });
                        }
                    }
                }

                return Json(new { success = true, data = logs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/GetSecurityStatistics
        [HttpGet]
        public JsonResult GetSecurityStatistics()
        {
            try
            {
                int activeUsers = 0;
                int adminActions = 0;

                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    // Active users trong 24h
                    var cmd1 = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                SELECT COUNT(DISTINCT MATK) 
                FROM AUDIT_LOG 
                WHERE HANHDONG IN ('LOGIN', 'LOGOUT') 
                AND NGAYGIO >= SYSDATE - 1", conn);
                    activeUsers = Convert.ToInt32(cmd1.ExecuteScalar());

                    // Admin actions hôm nay
                    var cmd2 = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                SELECT COUNT(*) 
                FROM AUDIT_LOG 
                WHERE BANGTACDONG = 'ADMIN_ACTION' 
                AND TRUNC(NGAYGIO) = TRUNC(SYSDATE)", conn);
                    adminActions = Convert.ToInt32(cmd2.ExecuteScalar());
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        activeUsers = activeUsers,
                        adminActions = adminActions
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/GetLoginLogs
        [HttpGet]
        public JsonResult GetLoginLogs()
        {
            try
            {
                var logs = new List<dynamic>();

                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                SELECT 
                    al.MALOG,
                    al.MATK,
                    al.HANHDONG,
                    al.NGAYGIO,
                    al.IP,
                    c.HOTEN,
                    c.EMAIL
                FROM AUDIT_LOG al
                LEFT JOIN CUSTOMER c ON al.MATK = c.MAKH
                WHERE al.HANHDONG IN ('LOGIN', 'LOGOUT', 'LOGIN_WITH_MAC')
                AND ROWNUM <= 100
                ORDER BY al.NGAYGIO DESC", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new
                            {
                                malog = Convert.ToInt32(reader["MALOG"]),
                                matk = reader["MATK"] != DBNull.Value ? Convert.ToInt32(reader["MATK"]) : (int?)null,
                                hanhdong = reader["HANHDONG"]?.ToString() ?? "",
                                ngaygio = Convert.ToDateTime(reader["NGAYGIO"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                ip = reader["IP"]?.ToString() ?? "",
                                hoten = reader["HOTEN"]?.ToString() ?? "",
                                email = reader["EMAIL"]?.ToString() ?? ""
                            });
                        }
                    }
                }

                return Json(new { success = true, data = logs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/GetAdminLogs
        [HttpGet]
        public JsonResult GetAdminLogs()
        {
            try
            {
                var logs = new List<dynamic>();

                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                SELECT 
                    al.MALOG,
                    al.MATK,
                    al.HANHDONG,
                    al.NGAYGIO,
                    c.HOTEN,
                    c.EMAIL
                FROM AUDIT_LOG al
                LEFT JOIN CUSTOMER c ON al.MATK = c.MAKH
                WHERE al.BANGTACDONG = 'ADMIN_ACTION'
                AND ROWNUM <= 100
                ORDER BY al.NGAYGIO DESC", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new
                            {
                                malog = Convert.ToInt32(reader["MALOG"]),
                                matk = reader["MATK"] != DBNull.Value ? Convert.ToInt32(reader["MATK"]) : (int?)null,
                                hanhdong = reader["HANHDONG"]?.ToString() ?? "",
                                ngaygio = Convert.ToDateTime(reader["NGAYGIO"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                hoten = reader["HOTEN"]?.ToString() ?? "",
                                email = reader["EMAIL"]?.ToString() ?? ""
                            });
                        }
                    }
                }

                return Json(new { success = true, data = logs }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/GetSecurityEvents
        [HttpGet]
        public JsonResult GetSecurityEvents()
        {
            try
            {
                var events = new List<dynamic>();

                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                SELECT 
                    al.MALOG,
                    al.MATK,
                    al.HANHDONG,
                    al.NGAYGIO,
                    al.IP,
                    c.HOTEN,
                    c.EMAIL
                FROM AUDIT_LOG al
                LEFT JOIN CUSTOMER c ON al.MATK = c.MAKH
                WHERE (
                    al.HANHDONG LIKE 'ACCESS_DENIED%'
                    OR al.HANHDONG LIKE '%FAILED%'
                    OR al.HANHDONG LIKE '%DENIED%'
                )
                AND al.NGAYGIO >= SYSDATE - 30
                AND ROWNUM <= 50
                ORDER BY al.NGAYGIO DESC", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            events.Add(new
                            {
                                malog = Convert.ToInt32(reader["MALOG"]),
                                matk = reader["MATK"] != DBNull.Value ? Convert.ToInt32(reader["MATK"]) : (int?)null,
                                hanhdong = reader["HANHDONG"]?.ToString() ?? "",
                                ngaygio = Convert.ToDateTime(reader["NGAYGIO"]).ToString("yyyy-MM-dd HH:mm:ss"),
                                ip = reader["IP"]?.ToString() ?? "",
                                hoten = reader["HOTEN"]?.ToString() ?? "",
                                email = reader["EMAIL"]?.ToString() ?? ""
                            });
                        }
                    }
                }

                return Json(new { success = true, data = events }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/ExportAuditLogs
        [HttpGet]
        public ActionResult ExportAuditLogs()
        {
            try
            {
                var logs = new List<string>();
                logs.Add("ID,User ID,User Name,Email,Action,Table,Time,IP"); // CSV Header

                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    conn.Open();

                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"
                SELECT 
                    al.MALOG,
                    al.MATK,
                    al.HANHDONG,
                    al.BANGTACDONG,
                    TO_CHAR(al.NGAYGIO, 'YYYY-MM-DD HH24:MI:SS') AS NGAYGIO,
                    al.IP,
                    c.HOTEN,
                    c.EMAIL
                FROM AUDIT_LOG al
                LEFT JOIN CUSTOMER c ON al.MATK = c.MAKH
                WHERE al.NGAYGIO >= SYSDATE - 30
                ORDER BY al.NGAYGIO DESC", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                                reader["MALOG"],
                                reader["MATK"] != DBNull.Value ? reader["MATK"].ToString() : "N/A",
                                reader["HOTEN"]?.ToString() ?? "N/A",
                                reader["EMAIL"]?.ToString() ?? "N/A",
                                reader["HANHDONG"]?.ToString() ?? "",
                                reader["BANGTACDONG"]?.ToString() ?? "",
                                reader["NGAYGIO"]?.ToString() ?? "",
                                reader["IP"]?.ToString() ?? "N/A"
                            );
                            logs.Add(line);
                        }
                    }
                }

                var csv = string.Join(Environment.NewLine, logs);
                var fileName = $"AuditLogs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi export: " + ex.Message;
                return RedirectToAction("Security");
            }
        }
    }

}