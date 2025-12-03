using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebCar.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string HOTEN { get; set; }

        [Required]
        [EmailAddress]
        public string EMAIL { get; set; }

        [Required]
        public string SDT { get; set; }

        [Required]
        public string DIACHI { get; set; }

        [Required]
        [MinLength(6)]
        public string MATKHAU { get; set; }

        [Required]
        [Compare("MATKHAU", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string XacNhanMatKhau { get; set; }
    }
}