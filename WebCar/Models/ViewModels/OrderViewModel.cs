using System;
using System.ComponentModel.DataAnnotations;

namespace WebCar.Models.ViewModels
{
    public class OrderViewModel
    {
        
        public int MaKH { get; set; }
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
        // Order info
        public int MaDon { get; set; }
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }

        // ✅ Encrypted shipping info (decrypted for display)
        public string DiaChiGiaoHang { get; set; }
        public string SoDienThoai { get; set; }
        public string GhiChu { get; set; }

        // Car info
        public int MaXe { get; set; }
        public string TenXe { get; set; }
        public string HangXe { get; set; }
        public string HinhAnh { get; set; }
        public string MoTa { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        // Computed
        public string TrangThaiClass
        {
            get
            {
                switch (TrangThai)
                {
                    case "Cho xu ly": return "badge bg-warning";
                    case "Dang xu ly": return "badge bg-info";
                    case "Hoan thanh": return "badge bg-success";
                    case "Da huy": return "badge bg-danger";
                    default: return "badge bg-secondary";
                }
            }
        }

        public string TrangThaiIcon
        {
            get
            {
                switch (TrangThai)
                {
                    case "Cho xu ly": return "fa-clock";
                    case "Dang xu ly": return "fa-spinner fa-spin";
                    case "Hoan thanh": return "fa-check-circle";
                    case "Da huy": return "fa-times-circle";
                    default: return "fa-question-circle";
                }
            }
        }
    }

    public class CreateOrderViewModel
    {
        public int MaXe { get; set; }

        [Display(Name = "Tên xe")]
        public string TenXe { get; set; }

        [Display(Name = "Hãng xe")]
        public string HangXe { get; set; }

        [Display(Name = "Giá")]
        public decimal Gia { get; set; }

        public string HinhAnh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, 10, ErrorMessage = "Số lượng từ 1-10")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }

        // ✅ OPTIONAL - Not required
        [Display(Name = "Địa chỉ giao hàng")]
        [StringLength(500, ErrorMessage = "Địa chỉ tối đa 500 ký tự")]
        public string DiaChiGiaoHang { get; set; }

        // ✅ OPTIONAL - More flexible regex
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^[0-9+\-\s()]{10,15}$", ErrorMessage = "Số điện thoại không hợp lệ (10-15 số)")]
        public string SoDienThoai { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(1000, ErrorMessage = "Ghi chú tối đa 1000 ký tự")]
        public string GhiChu { get; set; }

        // Helper properties
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