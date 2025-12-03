using System;
using System.Linq;
using System.Web.Mvc;
using WebCar.Data;
using WebCar.Models.ViewModels;

namespace WebCar.Controllers
{
    public class ProductController : Controller
    {
        private readonly CarRepository _carRepo;

        public ProductController()
        {
            _carRepo = new CarRepository();
        }

        // GET: Product/Index - Danh sách xe
        public ActionResult Index(string search, string brand, decimal? minPrice,
            decimal? maxPrice, short? year, int page = 1)
        {
            try
            {
                var pageSize = 12;

                // Lấy tất cả xe theo filter
                var allCars = _carRepo.GetAllCars(search, brand, minPrice, maxPrice, year);

                // Pagination
                var totalCars = allCars.Count;
                var totalPages = (int)Math.Ceiling((double)totalCars / pageSize);
                var cars = allCars.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var viewModel = new ProductListViewModel
                {
                    Cars = cars,
                    SearchTerm = search,
                    SelectedBrand = brand,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    Year = year,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    Brands = _carRepo.GetBrands()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải danh sách xe: " + ex.Message;
                return View(new ProductListViewModel());
            }
        }

        // GET: Product/Details - Chi tiết xe
        public ActionResult Details(decimal id)
        {
            try
            {
                var car = _carRepo.GetCarById(id);

                if (car == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy xe!";
                    return RedirectToAction("Index");
                }

                var viewModel = new ProductDetailViewModel
                {
                    Car = car,
                    RelatedCars = _carRepo.GetRelatedCars(id, car.HANGXE, 4),
                    IsAuthenticated = User.Identity.IsAuthenticated,
                    CanOrder = User.Identity.IsAuthenticated && car.TRANGTHAI == "Con hang"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải chi tiết xe: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}