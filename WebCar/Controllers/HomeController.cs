using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebCar.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

          public ActionResult Energy()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult LuuTruNL()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Oto()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
      
    }
}