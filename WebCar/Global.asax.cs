using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace WebCar
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var data = ticket.UserData.Split('|'); // MaKH | HoTen | Role

                string fullName = data[1];
                string role = data[2];

                HttpContext.Current.User =
                    new System.Security.Principal.GenericPrincipal(
                        new System.Security.Principal.GenericIdentity(fullName),
                        new[] { role }
                    );
            }
        }

    }
}
