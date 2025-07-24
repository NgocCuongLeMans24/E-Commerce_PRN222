using DAL_Group4_E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Group4MVC.Middleware
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public PermissionMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip permission check for non-authenticated users
            if (!context.User.Identity.IsAuthenticated)
            {
                await _next(context);
                return;
            }

            // Skip permission check for certain paths
            var path = context.Request.Path.Value?.ToLower();
            if (ShouldSkipPermissionCheck(path))
            {
                await _next(context);
                return;
            }

            // Get employee ID from claims
            var employeeId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(employeeId))
            {
                // Redirect to login if no employee ID found
                context.Response.Redirect("/Account/Login");
                return;
            }

            if (employeeId == "0")
            {
                await _next(context);
                return;
            }

            // Check permissions
            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EcommercePrn222Context>();

            var hasAccess = await CheckEmployeePermission(dbContext, employeeId, path, context.Request.Method);

            if (!hasAccess)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Access Denied: You don't have permission to access this resource.");
                return;
            }

            await _next(context);
        }

        private bool ShouldSkipPermissionCheck(string path)
        {
            var skipPaths = new[]
            {
                "/account/login",
                "/account/logout",
                "/account/register",
                "/account/forgotpassword",
                "/account/resetpassword",
                "/account/verifyotp",
                "/account/accessdenied",
                "/home/index",
                "/home/privacy",
                "/error",
                "/api/",
                "/css/",
                "/js/",
                "/images/",
                "/lib/"
            };

            return skipPaths.Any(skipPath => path?.StartsWith(skipPath) == true);
        }

        private async Task<bool> CheckEmployeePermission(EcommercePrn222Context context, string employeeId, string path, string httpMethod)
        {
            try
            {
                // Normalize the path for comparison
                var normalizedPath = path.ToLower().TrimStart('/');

                // Find matching web page by URL
                var webPages = await context.WebPages
                    .Where(w => w.Url != null)
                    .ToListAsync();
                var webPage = webPages.FirstOrDefault(w => w.Url.ToLower().TrimStart('/') == normalizedPath);

                if (webPage == null)
                    return true; // Allow access if page not in permission system

                // Get employee permissions for this page
                var permission = await context.Permissions
                    .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.PageId == webPage.PageId);

                if (permission == null)
                    return false; // No permission found, deny access

                // Check permission based on HTTP method and URL patterns
                return httpMethod.ToUpper() switch
                {
                    "GET" => permission.CanView,
                    "POST" when path.Contains("create") || path.Contains("add") => permission.CanAdd,
                    "POST" when path.Contains("edit") || path.Contains("update") => permission.CanEdit,
                    "POST" when path.Contains("delete") || path.Contains("remove") => permission.CanDelete,
                    "PUT" => permission.CanEdit,
                    "DELETE" => permission.CanDelete,
                    _ => permission.CanView
                };
            }
            catch
            {
                return false;
            }
        }
    }
}
