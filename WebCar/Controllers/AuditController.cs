using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using Oracle.ManagedDataAccess.Client;
using WebCar.Models.ViewModels;

namespace WebCar.Controllers
{
    [Authorize]
    public class AuditController : Controller
    {
        private readonly string _connectionString;

        public AuditController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["Model1"].ConnectionString;
        }

        // GET: Audit/MyLogs - Lịch sử hoạt động của user hiện tại
        public ActionResult MyLogs()
        {
            try
            {
                if (Session["CustomerId"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int customerId = (int)Session["CustomerId"];
                var logs = GetUserAuditLogs(customerId);

                ViewBag.CustomerName = Session["CustomerName"];
                ViewBag.TotalLogs = logs.Count;

                return View(logs);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải lịch sử: " + ex.Message;
                return View(new List<AuditLogViewModel>());
            }
        }

        // GET: Audit/Index - Tất cả logs (ADMIN only)
        [Authorize] // Thêm check role ADMIN sau
        public ActionResult Index(DateTime? fromDate, DateTime? toDate, string action = null)
        {
            try
            {
                var logs = GetAllAuditLogs(fromDate, toDate, action);

                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                ViewBag.SelectedAction = action;
                ViewBag.Actions = GetUniqueActions();

                return View(logs);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return View(new List<AuditLogViewModel>());
            }
        }

        #region Helper Methods

        private List<AuditLogViewModel> GetUserAuditLogs(int customerId)
        {
            var logs = new List<AuditLogViewModel>();

            using (var conn = new OracleConnection(_connectionString))
            {
                var cmd = new OracleCommand(@"
                    SELECT 
                        al.MALOG,
                        al.MATK,
                        c.HOTEN,
                        c.EMAIL,
                        al.HANHDONG,
                        al.BANGTACDONG,
                        al.NGAYGIO,
                        al.IP
                    FROM AUDIT_LOG al
                    LEFT JOIN CUSTOMER c ON al.MATK = c.MAKH
                    WHERE al.MATK = :customerId
                    ORDER BY al.NGAYGIO DESC
                    FETCH FIRST 100 ROWS ONLY", conn);

                cmd.Parameters.Add("customerId", OracleDbType.Int32).Value = customerId;

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(new AuditLogViewModel
                        {
                            MaLog = Convert.ToInt32(reader["MALOG"]),
                            MaTk = Convert.ToInt32(reader["MATK"]),
                            HoTen = reader["HOTEN"]?.ToString(),
                            Email = reader["EMAIL"]?.ToString(),
                            HanhDong = reader["HANHDONG"]?.ToString(),
                            BangTacDong = reader["BANGTACDONG"]?.ToString(),
                            NgayGio = Convert.ToDateTime(reader["NGAYGIO"]),
                            IP = reader["IP"]?.ToString()
                        });
                    }
                }
            }

            return logs;
        }

        private List<AuditLogViewModel> GetAllAuditLogs(DateTime? fromDate, DateTime? toDate, string action)
        {
            var logs = new List<AuditLogViewModel>();

            using (var conn = new OracleConnection(_connectionString))
            {
                var sql = @"
                    SELECT 
                        al.MALOG,
                        al.MATK,
                        c.HOTEN,
                        c.EMAIL,
                        ar.ROLENAME,
                        al.HANHDONG,
                        al.BANGTACDONG,
                        al.NGAYGIO,
                        al.IP
                    FROM AUDIT_LOG al
                    LEFT JOIN CUSTOMER c ON al.MATK = c.MAKH
                    LEFT JOIN ACCOUNT_ROLE ar ON al.MATK = ar.MATK
                    WHERE 1=1";

                if (fromDate.HasValue)
                    sql += " AND al.NGAYGIO >= :fromDate";

                if (toDate.HasValue)
                    sql += " AND al.NGAYGIO <= :toDate";

                if (!string.IsNullOrEmpty(action))
                    sql += " AND al.HANHDONG = :action";

                sql += " ORDER BY al.NGAYGIO DESC FETCH FIRST 200 ROWS ONLY";

                var cmd = new OracleCommand(sql, conn);

                if (fromDate.HasValue)
                    cmd.Parameters.Add("fromDate", OracleDbType.Date).Value = fromDate.Value;

                if (toDate.HasValue)
                    cmd.Parameters.Add("toDate", OracleDbType.Date).Value = toDate.Value.AddDays(1).AddSeconds(-1);

                if (!string.IsNullOrEmpty(action))
                    cmd.Parameters.Add("action", OracleDbType.Varchar2).Value = action;

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(new AuditLogViewModel
                        {
                            MaLog = Convert.ToInt32(reader["MALOG"]),
                            MaTk = Convert.ToInt32(reader["MATK"]),
                            HoTen = reader["HOTEN"]?.ToString(),
                            Email = reader["EMAIL"]?.ToString(),
                            RoleName = reader["ROLENAME"]?.ToString(),
                            HanhDong = reader["HANHDONG"]?.ToString(),
                            BangTacDong = reader["BANGTACDONG"]?.ToString(),
                            NgayGio = Convert.ToDateTime(reader["NGAYGIO"]),
                            IP = reader["IP"]?.ToString()
                        });
                    }
                }
            }

            return logs;
        }

        private List<string> GetUniqueActions()
        {
            var actions = new List<string>();

            using (var conn = new OracleConnection(_connectionString))
            {
                var cmd = new OracleCommand(@"
                    SELECT DISTINCT HANHDONG 
                    FROM AUDIT_LOG 
                    ORDER BY HANHDONG", conn);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        actions.Add(reader["HANHDONG"]?.ToString());
                    }
                }
            }

            return actions;
        }

        #endregion
    }
}