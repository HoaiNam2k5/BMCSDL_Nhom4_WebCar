using System;

namespace WebCar.Models.ViewModels
{
    public class AdminOrderViewModel
    {
        public int MaDon { get; set; }
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }

        // Chi tiết sản phẩm
        public int MaXe { get; set; }
        public string TenXe { get; set; }
        public string HangXe { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
}