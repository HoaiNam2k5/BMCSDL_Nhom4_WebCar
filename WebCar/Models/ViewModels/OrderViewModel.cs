using System;
using System.ComponentModel.DataAnnotations;

namespace WebCar.Models.ViewModels
{
    public class OrderViewModel
    {
        public int MaDon { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }

        // Thông tin xe
        public int MaXe { get; set; }
        public string TenXe { get; set; }
        public string HangXe { get; set; }
        public string HinhAnh { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        // Helper properties
        public string TrangThaiDisplay
        {
            get
            {
                switch (TrangThai)
                {
                    case "Cho xu ly": return "Chờ xử lý";
                    case "Dang xu ly": return "Đang xử lý";
                    case "Hoan thanh": return "Hoàn thành";
                    case "Da huy": return "Đã hủy";
                    default: return TrangThai;
                }
            }
        }

        public string TrangThaiBadgeClass
        {
            get
            {
                switch (TrangThai)
                {
                    case "Cho xu ly": return "bg-warning";
                    case "Dang xu ly": return "bg-info";
                    case "Hoan thanh": return "bg-success";
                    case "Da huy": return "bg-danger";
                    default: return "bg-secondary";
                }
            }
        }
    }

    public class OrderDetailViewModel
    {
        public int MaDon { get; set; }
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }

        // Chi tiết xe
   
        public int MaXe { get; set; }
        public string TenXe { get; set; }
        public string HangXe { get; set; }
        public string HinhAnh { get; set; }
        public string MoTa { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }

    public class CreateOrderViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn xe")]
        public int MaXe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, 10, ErrorMessage = "Số lượng phải từ 1-10")]
        public int SoLuong { get; set; }

        // Thông tin xe để hiển thị
        public string TenXe { get; set; }
        public string HangXe { get; set; }
        public decimal Gia { get; set; } // ✅ Đúng là decimal
        public string HinhAnh { get; set; }

        // Helper property
        public string GiaFormatted
        {
            get
            {
                return Gia.ToString("N0") + " VNĐ";
            }
        }

        public decimal TongTien
        {
            get
            {
                return Gia * SoLuong;
            }
        }
    }
}