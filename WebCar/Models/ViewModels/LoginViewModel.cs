using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebCar.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string EMAIL { get; set; }

        [Required]
        public string MATKHAU { get; set; }

        public bool RememberMe { get; set; }
    }
}