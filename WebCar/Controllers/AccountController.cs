using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebCar.Data;
using WebCar.Models;
using WebCar.Models.ViewModels;

namespace WebCar.Controllers
{
    public class AccountController : Controller
    {
        private readonly CustomerRepository _customerRepo;

        public AccountController()
        {
            _customerRepo = new CustomerRepository();
        }

        // =========================================
        // GET: Account/Register
        // =========================================
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            // ✅ SỬA: Chỉ kiểm tra Session, KHÔNG dùng User.Identity
            if (Session["CustomerId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // =========================================
        // POST: Account/Register
        // =========================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var result = _customerRepo.Register(model);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }

                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(model);
            }
        }

        // =========================================
        // GET: Account/Login
        // =========================================
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            System.Diagnostics.Debug.WriteLine("========== GET Login ==========");
            System.Diagnostics.Debug.WriteLine($"Session CustomerId: {Session["CustomerId"]}");
            System.Diagnostics.Debug.WriteLine($"User.Identity.IsAuthenticated: {User.Identity.IsAuthenticated}");

            // ✅ SỬA: Chỉ kiểm tra Session
            if (Session["CustomerId"] != null)
            {
                System.Diagnostics.Debug.WriteLine("User already logged in via Session");
                return RedirectToAction("Index", "Home");
            }

            System.Diagnostics.Debug.WriteLine("Returning Login View");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // =========================================
        // POST: Account/Login
        // =========================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("========== POST Login ==========");
                System.Diagnostics.Debug.WriteLine($"Email: {model.EMAIL}");

                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("ModelState invalid");
                    return View(model);
                }

                System.Diagnostics.Debug.WriteLine("Calling Repository Login...");
                var result = _customerRepo.Login(model);
                System.Diagnostics.Debug.WriteLine($"Login result: {result.Success}");

                if (result.Success)
                {
                    // Tạo Forms Authentication Ticket
                    var ticket = new FormsAuthenticationTicket(
                        1,
                        result.Customer.Email,
                        DateTime.Now,
                        DateTime.Now.AddHours(2),
                        model.RememberMe,
                        $"{result.Customer.MaKH}|{result.Customer.HoTen}|{result.Customer.RoleName}",
                        FormsAuthentication.FormsCookiePath
                    );

                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

                    if (model.RememberMe)
                    {
                        authCookie.Expires = ticket.Expiration;
                    }

                    Response.Cookies.Add(authCookie);

                    // Lưu Session
                    Session["CustomerId"] = result.Customer.MaKH;
                    Session["CustomerName"] = result.Customer.HoTen;
                    Session["CustomerEmail"] = result.Customer.Email;
                    Session["RoleName"] = result.Customer.RoleName; // ✅ SỬA: RoleName thay vì CustomerRole

                    System.Diagnostics.Debug.WriteLine("✅ Login successful, redirecting...");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                System.Diagnostics.Debug.WriteLine($"❌ Login failed: {result.Message}");
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 Exception: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                return View(model);
            }
        }

        // =========================================
        // POST: Account/Logout
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            try
            {
                if (Session["CustomerId"] != null)
                {
                    int customerId = (int)Session["CustomerId"];
                    _customerRepo.Logout(customerId);
                }

                Session.Clear();
                Session.Abandon();
                FormsAuthentication.SignOut();

                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName)
                    {
                        Expires = DateTime.Now.AddDays(-1)
                    };
                    Response.Cookies.Add(cookie);
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi đăng xuất: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // =========================================
        // GET: Account/MyProfile
        // =========================================
        [HttpGet]
        [Authorize]
        public ActionResult MyProfile()
        {
            try
            {
                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login");
                }

                int customerId = (int)Session["CustomerId"];
                var customer = _customerRepo.GetCustomerById(customerId);

                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin tài khoản!";
                    return RedirectToAction("Login");
                }

                ViewBag.ActivityCount = GetActivityCount(customerId);
                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải hồ sơ: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // =========================================
        // POST: Account/UpdateProfile
        // =========================================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(CUSTOMER model)
        {
            try
            {
                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login");
                }

                int customerId = (int)Session["CustomerId"];
                model.MAKH = customerId;

                var result = _customerRepo.UpdateCustomer(model);

                if (result.Success)
                {
                    Session["CustomerName"] = model.HOTEN;
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("MyProfile");
                }

                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("MyProfile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật: " + ex.Message;
                return RedirectToAction("MyProfile");
            }
        }

        // =========================================
        // GET: Account/ChangePassword
        // =========================================
        [HttpGet]
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        // =========================================
        // POST: Account/ChangePassword
        // =========================================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login");
                }

                int customerId = (int)Session["CustomerId"];
                var result = _customerRepo.ChangePassword(customerId, model.OldPassword, model.NewPassword);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("MyProfile");
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

        // =========================================
        // GET: Account/AccessDenied
        // =========================================
        [HttpGet]
        public ActionResult AccessDenied()
        {
            return View();
        }

        // =========================================
        // Helper: Get Activity Count
        // =========================================
        private int GetActivityCount(int customerId)
        {
            try
            {
                using (var conn = new Oracle.ManagedDataAccess.Client.OracleConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                {
                    var cmd = new Oracle.ManagedDataAccess.Client.OracleCommand(
                        "SELECT COUNT(*) FROM AUDIT_LOG WHERE MATK = :customerId", conn);

                    cmd.Parameters.Add("customerId", Oracle.ManagedDataAccess.Client.OracleDbType.Int32).Value = customerId;

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}