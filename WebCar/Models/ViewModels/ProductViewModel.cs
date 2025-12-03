using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebCar.Models.ViewModels
{
    // ViewModel cho danh sách sản phẩm
    public class ProductListViewModel
    {
        public List<CAR> Cars { get; set; }
        public string SearchTerm { get; set; }
        public string SelectedBrand { get; set; }
        public decimal? MinPrice { get; set; } // ✅ FIX: decimal?
        public decimal? MaxPrice { get; set; } // ✅ FIX: decimal?
        public short? Year { get; set; } // ✅ FIX: short?

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 12;

        // Brands for filter
        public List<string> Brands { get; set; }
    }

    // ViewModel cho chi tiết sản phẩm
    public class ProductDetailViewModel
    {
        public CAR Car { get; set; }
        public List<CAR> RelatedCars { get; set; }
        public List<FEEDBACK> Feedbacks { get; set; }
        public bool CanOrder { get; set; }
        public bool IsAuthenticated { get; set; }
    }
}