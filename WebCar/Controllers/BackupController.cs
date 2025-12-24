using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebCar.Filters;
using Oracle.ManagedDataAccess.Client;

namespace WebCar.Controllers
{
    [AuthorizeRole("ADMIN")]
    public class BackupController : Controller
    {
        private readonly string _connectionString;

        public BackupController()
        {
            _connectionString = System.Configuration.ConfigurationManager
                .ConnectionStrings["Model1"].ConnectionString;
        }

        // ==================== VIEWS ====================

        // GET: Backup/Index
        public ActionResult Index()
        {
            try
            {
                var backups = GetBackupList();

                ViewBag.TotalBackups = backups.Count;
                ViewBag.TodayBackups = backups.Count(b => b.CreatedDate.Date == DateTime.Today);
                ViewBag.WeekBackups = backups.Count(b => b.CreatedDate >= DateTime.Today.AddDays(-7));
                ViewBag.CompletedBackups = backups.Count(b => b.Status == "COMPLETED");
                ViewBag.FailedBackups = backups.Count(b => b.Status == "FAILED");

                return View(backups);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error:  " + ex.Message;
                return View(new List<BackupInfo>());
            }
        }

        // ==================== BACKUP ACTIONS ====================

        // POST: Backup/CreateBackup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateBackup(string backupName)
        {
            try
            {
                if (Session["CustomerId"] == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                int userId = (int)Session["CustomerId"];

                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    var cmd = new OracleCommand("SP_CREATE_BACKUP", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    // Input parameters
                    cmd.Parameters.Add("p_backup_name", OracleDbType.Varchar2).Value =
                        string.IsNullOrEmpty(backupName) ? "WEB_BACKUP" : backupName;

                    cmd.Parameters.Add("p_created_by", OracleDbType.Int32).Value = userId;

                    // Output parameters
                    cmd.Parameters.Add("p_backup_id", OracleDbType.Int32).Direction =
                        System.Data.ParameterDirection.Output;

                    cmd.Parameters.Add("p_result", OracleDbType.Int32).Direction =
                        System.Data.ParameterDirection.Output;

                    cmd.Parameters.Add("p_message", OracleDbType.Varchar2, 4000).Direction =
                        System.Data.ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    int backupId = Convert.ToInt32(cmd.Parameters["p_backup_id"].Value.ToString());
                    int result = Convert.ToInt32(cmd.Parameters["p_result"].Value.ToString());
                    string message = cmd.Parameters["p_message"].Value.ToString();

                    if (result == 1)
                    {
                        return Json(new
                        {
                            success = true,
                            message = message,
                            backupId = backupId
                        });
                    }

                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateBackup Error: {ex.Message}");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Backup/RestoreBackup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RestoreBackup(int backupId)
        {
            try
            {
                if (Session["CustomerId"] == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                int userId = (int)Session["CustomerId"];

                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    var cmd = new OracleCommand("SP_RESTORE_BACKUP", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_backup_id", OracleDbType.Int32).Value = backupId;
                    cmd.Parameters.Add("p_restored_by", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_result", OracleDbType.Int32).Direction =
                        System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add("p_message", OracleDbType.Varchar2, 4000).Direction =
                        System.Data.ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    int result = Convert.ToInt32(cmd.Parameters["p_result"].Value.ToString());
                    string message = cmd.Parameters["p_message"].Value.ToString();

                    if (result == 1)
                    {
                        return Json(new { success = true, message = message });
                    }

                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Backup/DeleteBackup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteBackup(int backupId)
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    var cmd = new OracleCommand("SP_DELETE_BACKUP", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_backup_id", OracleDbType.Int32).Value = backupId;
                    cmd.Parameters.Add("p_result", OracleDbType.Int32).Direction =
                        System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add("p_message", OracleDbType.Varchar2, 4000).Direction =
                        System.Data.ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    int result = Convert.ToInt32(cmd.Parameters["p_result"].Value.ToString());
                    string message = cmd.Parameters["p_message"].Value.ToString();

                    if (result == 1)
                    {
                        return Json(new { success = true, message = message });
                    }

                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Backup/CleanupOldBackups
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CleanupOldBackups(int days = 30)
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    var cmd = new OracleCommand("SP_CLEANUP_OLD_BACKUPS", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_days", OracleDbType.Int32).Value = days;
                    cmd.Parameters.Add("p_result", OracleDbType.Int32).Direction =
                        System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add("p_message", OracleDbType.Varchar2, 4000).Direction =
                        System.Data.ParameterDirection.Output;

                    cmd.ExecuteNonQuery();

                    int result = Convert.ToInt32(cmd.Parameters["p_result"].Value.ToString());
                    string message = cmd.Parameters["p_message"].Value.ToString();

                    if (result == 1)
                    {
                        return Json(new { success = true, message = message });
                    }

                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // GET: Backup/GetBackupDetails
        [HttpGet]
        public JsonResult GetBackupDetails(int backupId)
        {
            try
            {
                var details = new List<dynamic>();

                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    var cmd = new OracleCommand("SP_GET_BACKUP_DETAILS", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_backup_id", OracleDbType.Int32).Value = backupId;
                    cmd.Parameters.Add("p_details", OracleDbType.RefCursor).Direction =
                        System.Data.ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            details.Add(new
                            {
                                tableName = reader["TABLE_NAME"].ToString(),
                                recordCount = Convert.ToInt32(reader["RECORD_COUNT"]),
                                backupTableName = reader["BACKUP_TABLE_NAME"].ToString()
                            });
                        }
                    }
                }

                return Json(new { success = true, data = details }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Backup/GetBackupStats
        [HttpGet]
        public JsonResult GetBackupStats()
        {
            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    var cmd = new OracleCommand(@"
                        SELECT 
                            COUNT(*) AS TOTAL,
                            COUNT(CASE WHEN BACKUP_STATUS = 'COMPLETED' THEN 1 END) AS COMPLETED,
                            COUNT(CASE WHEN BACKUP_STATUS = 'FAILED' THEN 1 END) AS FAILED,
                            COUNT(CASE WHEN TRUNC(CREATED_DATE) = TRUNC(SYSDATE) THEN 1 END) AS TODAY,
                            SUM(TOTAL_RECORDS) AS TOTAL_RECORDS,
                            SUM(BACKUP_SIZE_MB) AS TOTAL_SIZE_MB
                        FROM BACKUP_METADATA", conn);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Json(new
                            {
                                success = true,
                                data = new
                                {
                                    total = Convert.ToInt32(reader["TOTAL"]),
                                    completed = Convert.ToInt32(reader["COMPLETED"]),
                                    failed = Convert.ToInt32(reader["FAILED"]),
                                    today = Convert.ToInt32(reader["TODAY"]),
                                    totalRecords = reader["TOTAL_RECORDS"] != DBNull.Value ?
                                        Convert.ToInt32(reader["TOTAL_RECORDS"]) : 0,
                                    totalSizeMB = reader["TOTAL_SIZE_MB"] != DBNull.Value ?
                                        Convert.ToDouble(reader["TOTAL_SIZE_MB"]) : 0
                                }
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // ==================== HELPER METHODS ====================

        private List<BackupInfo> GetBackupList()
        {
            var backups = new List<BackupInfo>();

            try
            {
                using (var conn = new OracleConnection(_connectionString))
                {
                    conn.Open();

                    var cmd = new OracleCommand("SP_LIST_BACKUPS", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_backups", OracleDbType.RefCursor).Direction =
                        System.Data.ParameterDirection.Output;

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            backups.Add(new BackupInfo
                            {
                                BackupId = Convert.ToInt32(reader["BACKUP_ID"]),
                                BackupName = reader["BACKUP_NAME"]?.ToString() ?? "",
                                BackupType = reader["BACKUP_TYPE"]?.ToString() ?? "",
                                Status = reader["BACKUP_STATUS"]?.ToString() ?? "",
                                CreatedBy = reader["CREATED_BY"] != DBNull.Value ?
                                    Convert.ToInt32(reader["CREATED_BY"]) : 0,
                                CreatedByName = reader["CREATED_BY_NAME"]?.ToString() ?? "",
                                CreatedDate = Convert.ToDateTime(reader["CREATED_DATE"]),
                                TotalTables = reader["TOTAL_TABLES"] != DBNull.Value ?
                                    Convert.ToInt32(reader["TOTAL_TABLES"]) : 0,
                                TotalRecords = reader["TOTAL_RECORDS"] != DBNull.Value ?
                                    Convert.ToInt32(reader["TOTAL_RECORDS"]) : 0,
                                SizeMB = reader["BACKUP_SIZE_MB"] != DBNull.Value ?
                                    Convert.ToDouble(reader["BACKUP_SIZE_MB"]) : 0,
                                Notes = reader["NOTES"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetBackupList: {ex.Message}");
            }

            return backups;
        }
    }

    // ==================== MODEL ====================
    public class BackupInfo
    {
        public int BackupId { get; set; }
        public string BackupName { get; set; }
        public string BackupType { get; set; }
        public string Status { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalTables { get; set; }
        public int TotalRecords { get; set; }
        public double SizeMB { get; set; }
        public string Notes { get; set; }

        public string StatusBadgeClass
        {
            get
            {
                switch (Status)
                {
                    case "COMPLETED": return "bg-success";
                    case "RUNNING": return "bg-warning";
                    case "FAILED": return "bg-danger";
                    default: return "bg-secondary";
                }
            }
        }

        public string StatusIcon
        {
            get
            {
                switch (Status)
                {
                    case "COMPLETED": return "fa-check-circle";
                    case "RUNNING": return "fa-spinner fa-spin";
                    case "FAILED": return "fa-times-circle";
                    default: return "fa-question-circle";
                }
            }
        }

        public string AgeFormatted
        {
            get
            {
                var age = DateTime.Now - CreatedDate;
                if (age.TotalHours < 1) return $"{(int)age.TotalMinutes} phút trước";
                if (age.TotalDays < 1) return $"{(int)age.TotalHours} giờ trước";
                if (age.TotalDays < 7) return $"{(int)age.TotalDays} ngày trước";
                return CreatedDate.ToString("dd/MM/yyyy HH:mm");
            }
        }
    }
}