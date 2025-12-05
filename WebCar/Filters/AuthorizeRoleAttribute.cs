using System;
using System.Web;
using System.Web.Mvc;

namespace WebCar.Filters
{
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Kiểm tra đã login chưa
            if (httpContext.Session["CustomerId"] == null)
            {
                return false;
            }

            // Nếu không có role nào được chỉ định, cho phép tất cả user đã login
            if (_allowedRoles == null || _allowedRoles.Length == 0)
            {
                return true;
            }

            // Kiểm tra role
            var userRole = httpContext.Session["RoleName"]?.ToString() ?? "CUSTOMER";

            foreach (var role in _allowedRoles)
            {
                if (userRole.Equals(role, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["CustomerId"] == null)
            {
                // Chưa login → redirect đến Login
                filterContext.Result = new RedirectResult("/Account/Login");
            }
            else
            {
                // Đã login nhưng không có quyền → redirect đến AccessDenied
                filterContext.Result = new RedirectResult("/Account/AccessDenied");
            }
        }
    }
}