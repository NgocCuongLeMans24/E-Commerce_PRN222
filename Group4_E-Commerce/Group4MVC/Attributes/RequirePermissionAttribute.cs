using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DAL_Group4_E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Group4MVC.Attributes
{
    public class RequirePermissionAttribute : ActionFilterAttribute
    {
        private readonly string _action;
        private readonly string _pageUrl;

        public RequirePermissionAttribute(string action, string pageUrl = null)
        {
            _action = action;
            _pageUrl = pageUrl;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;

            // Skip if user is not authenticated
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var employeeId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(employeeId))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (employeeId == "0") await next();

            var dbContext = httpContext.RequestServices.GetRequiredService<EcommercePrn222Context>();
            var pageUrl = _pageUrl ?? GetPageUrlFromContext(context);
            var hasPermission = await CheckPermission(dbContext, employeeId, _action, pageUrl);

            if (!hasPermission)
            {
                context.Result = new ViewResult
                {
                    ViewName = "AccessDenied",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
                        new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
                        new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                    {
                        ["Message"] = $"You don't have permission to {_action} on this page."
                    }
                };
                return;
            }

            await next();
        }

        private async Task<bool> CheckPermission(EcommercePrn222Context context, string employeeId, string action, string pageUrl)
        {
            try
            {
                // Normalize the URL for comparison
                var normalizedUrl = pageUrl.ToLower().TrimStart('/');

                // Find the web page by URL
                var webPages = await context.WebPages
                    .Where(w => w.Url != null)
                    .ToListAsync();
                var webPage = webPages.FirstOrDefault(w => w.Url.ToLower().TrimStart('/') == normalizedUrl);

                if (webPage == null)
                    return true; // Allow if page not in system

                // Get employee permission for this page
                var permission = await context.Permissions
                    .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.PageId == webPage.PageId);

                if (permission == null)
                    return false; // No permission found, deny access

                return action.ToLower() switch
                {
                    "view" => permission.CanView,
                    "add" or "create" => permission.CanAdd,
                    "edit" or "update" => permission.CanEdit,
                    "delete" or "remove" => permission.CanDelete,
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        private string GetPageUrlFromContext(ActionExecutingContext context)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString()?.ToLower();
            var actionName = context.RouteData.Values["action"]?.ToString()?.ToLower();

            return controllerName switch
            {
                "admin" when actionName == "customers" => "/admin/customers",
                "admin" when actionName == "employees" => "/admin/employees",
                "admin" when actionName == "permissions" => "/admin/permissions",
                "admin" when actionName == "webpages" => "/admin/webpages",
                "admin" when actionName == "index" => "/admin",
                "product" => "/product",
                "order" => "/order",
                _ => $"/{controllerName}"
            };
        }
    }
}
