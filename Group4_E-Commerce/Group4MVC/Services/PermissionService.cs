using DAL_Group4_E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ViewModels;

namespace Group4MVC.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly EcommercePrn222Context _context;

        public PermissionService(EcommercePrn222Context context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(string employeeId, string pageUrl, string action)
        {
            if (string.IsNullOrEmpty(employeeId) || string.IsNullOrEmpty(pageUrl) || string.IsNullOrEmpty(action))
                return false;
            Debug.WriteLine("huh??");
            try
            {
                // Normalize the URL for comparison
                var normalizedUrl = pageUrl.ToLower().TrimStart('/');
                Debug.WriteLine($"url :{normalizedUrl}");
                // Find the web page by URL
                var webPages = await _context.WebPages
                    .Where(w => w.Url != null)
                    .ToListAsync();
                var webPage = webPages.FirstOrDefault(w => w.Url.ToLower().TrimStart('/') == normalizedUrl);
                
                Debug.WriteLine($"web url: {_context.WebPages.First().Url.ToLower().TrimStart('/')}");
                if (webPage == null)
                    return true; // Allow access if page not in permission system
                Debug.WriteLine("web page not null");
                // Get employee permission for this page
                var permission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.PageId == webPage.PageId);

                if (permission == null)
                    return false; // No permission found, deny access
                Debug.WriteLine("permission not null");
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

        public async Task<List<string>> GetAccessiblePageUrlsAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return new List<string>();

            try
            {
                var accessibleUrls = await _context.Permissions
                    .Include(p => p.Page)
                    .Where(p => p.EmployeeId == employeeId && p.CanView)
                    .Select(p => p.Page.Url)
                    .ToListAsync();

                return accessibleUrls;
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<Dictionary<string, Dictionary<string, bool>>> GetEmployeePermissionsAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return new Dictionary<string, Dictionary<string, bool>>();

            try
            {
                var permissions = await _context.Permissions
                    .Include(p => p.Page)
                    .Where(p => p.EmployeeId == employeeId)
                    .ToListAsync();

                var result = new Dictionary<string, Dictionary<string, bool>>();

                foreach (var permission in permissions)
                {
                    if (permission.Page != null)
                    {
                        result[permission.Page.Url] = new Dictionary<string, bool>
                        {
                            ["view"] = permission.CanView,
                            ["add"] = permission.CanAdd,
                            ["edit"] = permission.CanEdit,
                            ["delete"] = permission.CanDelete
                        };
                    }
                }

                return result;
            }
            catch
            {
                return new Dictionary<string, Dictionary<string, bool>>();
            }
        }

        public async Task<List<Permission>> GetEmployeePermissionsForPageAsync(string employeeId, int pageId)
        {
            try
            {
                return await _context.Permissions
                    .Where(p => p.EmployeeId == employeeId && p.PageId == pageId)
                    .ToListAsync();
            }
            catch
            {
                return new List<Permission>();
            }
        }

        public async Task<bool> UpdateEmployeePermissionsAsync(string employeeId, Dictionary<int, PagePermissionViewModel> permissions)
        {
            try
            {
                // Get existing permissions for the employee
                var existingPermissions = await _context.Permissions
                    .Where(p => p.EmployeeId == employeeId)
                    .ToListAsync();

                // Update or create permissions for each page
                foreach (var pagePermission in permissions)
                {
                    var existingPermission = existingPermissions
                        .FirstOrDefault(p => p.PageId == pagePermission.Key);

                    if (existingPermission != null)
                    {
                        // Update existing permission
                        existingPermission.CanView = pagePermission.Value.CanView;
                        existingPermission.CanAdd = pagePermission.Value.CanAdd;
                        existingPermission.CanEdit = pagePermission.Value.CanEdit;
                        existingPermission.CanDelete = pagePermission.Value.CanDelete;
                        _context.Permissions.Update(existingPermission);
                    }
                    else
                    {
                        // Create new permission
                        var newPermission = new Permission
                        {
                            EmployeeId = employeeId,
                            PageId = pagePermission.Key,
                            CanView = pagePermission.Value.CanView,
                            CanAdd = pagePermission.Value.CanAdd,
                            CanEdit = pagePermission.Value.CanEdit,
                            CanDelete = pagePermission.Value.CanDelete
                        };
                        _context.Permissions.Add(newPermission);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<WebPage> GetPageByUrlAsync(string url)
        {
            try
            {
                var normalizedUrl = url.ToLower().TrimStart('/');
                return await _context.WebPages
                    .FirstOrDefaultAsync(w => w.Url.ToLower().TrimStart('/') == normalizedUrl);
            }
            catch
            {
                return null;
            }
        }
    }
}
