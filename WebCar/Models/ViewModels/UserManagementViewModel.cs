using System;

namespace WebCar.Models.ViewModels
{
    public class UserManagementViewModel
    {
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
        public DateTime NgayDangKy { get; set; }
        public string RoleName { get; set; }
        public int TotalOrders { get; set; }
        public int TotalActivities { get; set; }
    }
}