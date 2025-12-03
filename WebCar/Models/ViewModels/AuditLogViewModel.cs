using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebCar.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho một dòng audit log
    /// </summary>
    public class AuditLogViewModel
    {
        public int MaLog { get; set; }
        public int MaTk { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public string HanhDong { get; set; }
        public string BangTacDong { get; set; }
        public DateTime NgayGio { get; set; }
        public string IP { get; set; }

        // Helper properties - COMPATIBLE WITH C# 7.3
        public string HanhDongDisplay
        {
            get
            {
                switch (HanhDong)
                {
                    case "LOGIN": return "Đăng nhập";
                    case "LOGOUT": return "Đăng xuất";
                    case "REGISTER": return "Đăng ký";
                    case "INSERT": return "Thêm mới";
                    case "UPDATE": return "Cập nhật";
                    case "DELETE": return "Xóa";
                    case "CHANGE_PASSWORD": return "Đổi mật khẩu";
                    case "ACCESS_DENIED": return "Từ chối truy cập";
                    default: return HanhDong;
                }
            }
        }

        public string BadgeClass
        {
            get
            {
                switch (HanhDong)
                {
                    case "LOGIN": return "bg-success";
                    case "LOGOUT": return "bg-secondary";
                    case "REGISTER": return "bg-primary";
                    case "INSERT": return "bg-info";
                    case "UPDATE": return "bg-warning";
                    case "DELETE": return "bg-danger";
                    case "CHANGE_PASSWORD": return "bg-dark";
                    case "ACCESS_DENIED": return "bg-danger";
                    default: return "bg-light text-dark";
                }
            }
        }

        public string IconClass
        {
            get
            {
                switch (HanhDong)
                {
                    case "LOGIN": return "fa-sign-in-alt";
                    case "LOGOUT": return "fa-sign-out-alt";
                    case "REGISTER": return "fa-user-plus";
                    case "INSERT": return "fa-plus";
                    case "UPDATE": return "fa-edit";
                    case "DELETE": return "fa-trash";
                    case "CHANGE_PASSWORD": return "fa-key";
                    case "ACCESS_DENIED": return "fa-ban";
                    default: return "fa-info-circle";
                }
            }
        }

        public string TimeAgo
        {
            get
            {
                var span = DateTime.Now - NgayGio;

                if (span.TotalMinutes < 1)
                    return "Vừa xong";
                else if (span.TotalMinutes < 60)
                    return string.Format("{0} phút trước", (int)span.TotalMinutes);
                else if (span.TotalHours < 24)
                    return string.Format("{0} giờ trước", (int)span.TotalHours);
                else if (span.TotalDays < 30)
                    return string.Format("{0} ngày trước", (int)span.TotalDays);
                else
                    return NgayGio.ToString("dd/MM/yyyy");
            }
        }
    }

    /// <summary>
    /// ViewModel cho trang danh sách audit logs với filter
    /// </summary>
    public class AuditLogListViewModel
    {
        public List<AuditLogViewModel> Logs { get; set; }
        public List<string> Actions { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SelectedAction { get; set; }
        public string SearchEmail { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalLogs { get; set; }

        public AuditLogListViewModel()
        {
            Logs = new List<AuditLogViewModel>();
            Actions = new List<string>();
            CurrentPage = 1;
            TotalPages = 1;
        }
    }

    /// <summary>
    /// ViewModel cho dashboard audit (dành cho admin)
    /// </summary>
    public class AuditDashboardViewModel
    {
        public int TotalLogs { get; set; }
        public int TodayLogs { get; set; }
        public int ThisWeekLogs { get; set; }
        public int ThisMonthLogs { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsersToday { get; set; }
        public int TotalLogins { get; set; }
        public int FailedAttempts { get; set; }
        public List<AuditLogViewModel> RecentLogs { get; set; }
        public List<TopUserActivity> TopUsers { get; set; }
        public List<ActionStatistic> ActionStatistics { get; set; }
        public List<DailyActivityStatistic> DailyActivities { get; set; }

        public AuditDashboardViewModel()
        {
            RecentLogs = new List<AuditLogViewModel>();
            TopUsers = new List<TopUserActivity>();
            ActionStatistics = new List<ActionStatistic>();
            DailyActivities = new List<DailyActivityStatistic>();
        }
    }

    /// <summary>
    /// Top user có nhiều hoạt động nhất
    /// </summary>
    public class TopUserActivity
    {
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public int ActivityCount { get; set; }
        public int LoginCount { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    /// <summary>
    /// Thống kê theo loại hành động
    /// </summary>
    public class ActionStatistic
    {
        public string Action { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string DisplayName { get; set; }
        public string ColorClass { get; set; }
    }

    /// <summary>
    /// Thống kê hoạt động theo ngày
    /// </summary>
    public class DailyActivityStatistic
    {
        public DateTime Date { get; set; }
        public int LoginCount { get; set; }
        public int RegisterCount { get; set; }
        public int UpdateCount { get; set; }
        public int TotalCount { get; set; }

        public string DateDisplay
        {
            get { return Date.ToString("dd/MM"); }
        }
    }

    /// <summary>
    /// ViewModel cho security events
    /// </summary>
    public class SecurityEventViewModel
    {
        public string EventType { get; set; }
        public string EventTypeName { get; set; }
        public int Count { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public string SeverityClass { get; set; }
        public DateTime? LastOccurrence { get; set; }
        public List<AuditLogViewModel> RecentEvents { get; set; }

        public SecurityEventViewModel()
        {
            RecentEvents = new List<AuditLogViewModel>();
        }
    }

    /// <summary>
    /// ViewModel cho compliance report
    /// </summary>
    public class ComplianceReportViewModel
    {
        public DateTime ReportDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalAuthenticationEvents { get; set; }
        public int SuccessfulLogins { get; set; }
        public int FailedLogins { get; set; }
        public int Logouts { get; set; }
        public int UniqueUsers { get; set; }
        public int DataModifications { get; set; }
        public int Inserts { get; set; }
        public int Updates { get; set; }
        public int Deletes { get; set; }
        public int AuthorizationChanges { get; set; }
        public int PasswordChanges { get; set; }
        public List<string> Recommendations { get; set; }

        public ComplianceReportViewModel()
        {
            ReportDate = DateTime.Now;
            Recommendations = new List<string>();
        }
    }

    /// <summary>
    /// ViewModel cho anomaly detection
    /// </summary>
    public class AnomalyDetectionViewModel
    {
        public string AnomalyType { get; set; }
        public int MaKH { get; set; }
        public string Email { get; set; }
        public string HoTen { get; set; }
        public int SuspiciousCount { get; set; }
        public string Details { get; set; }
        public DateTime DetectedAt { get; set; }
        public string Severity { get; set; }
        public List<AuditLogViewModel> RelatedLogs { get; set; }

        public AnomalyDetectionViewModel()
        {
            RelatedLogs = new List<AuditLogViewModel>();
        }
    }

    /// <summary>
    /// ViewModel cho user activity timeline
    /// </summary>
    public class UserActivityTimelineViewModel
    {
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public List<TimelineEvent> Events { get; set; }
        public Dictionary<string, int> ActivitySummary { get; set; }

        public UserActivityTimelineViewModel()
        {
            Events = new List<TimelineEvent>();
            ActivitySummary = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Timeline event
    /// </summary>
    public class TimelineEvent
    {
        public DateTime Time { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public string IconClass { get; set; }
        public string ColorClass { get; set; }
        public string IP { get; set; }
    }

    /// <summary>
    /// ViewModel cho filter audit logs
    /// </summary>
    public class AuditLogFilterViewModel
    {
        [Display(Name = "Từ ngày")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Đến ngày")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Hành động")]
        public string Action { get; set; }

        [Display(Name = "Email người dùng")]
        public string UserEmail { get; set; }

        [Display(Name = "IP Address")]
        public string IPAddress { get; set; }

        [Display(Name = "Bảng")]
        public string TableName { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }

        public AuditLogFilterViewModel()
        {
            Page = 1;
            PageSize = 50;
        }
    }

    /// <summary>
    /// Helper class để format và xử lý audit log data
    /// </summary>
    public static class AuditLogHelper
    {
        public static string GetActionDisplayName(string action)
        {
            if (action == null) return "";

            switch (action)
            {
                case "LOGIN": return "Đăng nhập";
                case "LOGOUT": return "Đăng xuất";
                case "REGISTER": return "Đăng ký";
                case "INSERT": return "Thêm mới";
                case "UPDATE": return "Cập nhật";
                case "DELETE": return "Xóa";
                case "CHANGE_PASSWORD": return "Đổi mật khẩu";
                case "ACCESS_DENIED": return "Từ chối truy cập";
                case "BACKUP": return "Sao lưu";
                case "RESTORE": return "Khôi phục";
                case "GRANT_ROLE": return "Cấp quyền";
                case "REVOKE_ROLE": return "Thu hồi quyền";
                default: return action;
            }
        }

        public static string GetBadgeClass(string action)
        {
            if (action == null) return "bg-light text-dark";

            switch (action)
            {
                case "LOGIN": return "bg-success";
                case "LOGOUT": return "bg-secondary";
                case "REGISTER": return "bg-primary";
                case "INSERT": return "bg-info";
                case "UPDATE": return "bg-warning text-dark";
                case "DELETE": return "bg-danger";
                case "CHANGE_PASSWORD": return "bg-dark";
                case "ACCESS_DENIED": return "bg-danger";
                case "BACKUP": return "bg-info";
                case "RESTORE": return "bg-warning";
                case "GRANT_ROLE": return "bg-success";
                case "REVOKE_ROLE": return "bg-danger";
                default: return "bg-light text-dark";
            }
        }

        public static string GetIconClass(string action)
        {
            if (action == null) return "fa-info-circle";

            switch (action)
            {
                case "LOGIN": return "fa-sign-in-alt";
                case "LOGOUT": return "fa-sign-out-alt";
                case "REGISTER": return "fa-user-plus";
                case "INSERT": return "fa-plus";
                case "UPDATE": return "fa-edit";
                case "DELETE": return "fa-trash";
                case "CHANGE_PASSWORD": return "fa-key";
                case "ACCESS_DENIED": return "fa-ban";
                case "BACKUP": return "fa-save";
                case "RESTORE": return "fa-undo";
                case "GRANT_ROLE": return "fa-user-shield";
                case "REVOKE_ROLE": return "fa-user-slash";
                default: return "fa-info-circle";
            }
        }

        public static string FormatTimeAgo(DateTime dateTime)
        {
            var span = DateTime.Now - dateTime;

            if (span.TotalSeconds < 60)
                return "Vừa xong";
            else if (span.TotalMinutes < 60)
                return string.Format("{0} phút trước", (int)span.TotalMinutes);
            else if (span.TotalHours < 24)
                return string.Format("{0} giờ trước", (int)span.TotalHours);
            else if (span.TotalDays < 7)
                return string.Format("{0} ngày trước", (int)span.TotalDays);
            else if (span.TotalDays < 30)
                return string.Format("{0} tuần trước", (int)(span.TotalDays / 7));
            else if (span.TotalDays < 365)
                return string.Format("{0} tháng trước", (int)(span.TotalDays / 30));
            else
                return string.Format("{0} năm trước", (int)(span.TotalDays / 365));
        }
    }
}