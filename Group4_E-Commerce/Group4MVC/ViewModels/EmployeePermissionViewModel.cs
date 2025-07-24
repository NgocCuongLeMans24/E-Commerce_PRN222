using DAL_Group4_E_Commerce.Models;
using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class EmployeePermissionViewModel
    {
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public List<WebPage> WebPages { get; set; } = new List<WebPage>();
        public List<Permission> Permissions { get; set; } = new List<Permission>();

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Filters
        public string SearchTerm { get; set; } = string.Empty;
        public int? PageFilter { get; set; }
    }

    public class UpdateEmployeePermissionsViewModel
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        public Dictionary<int, PagePermissionViewModel> Permissions { get; set; } = new Dictionary<int, PagePermissionViewModel>();
    }

    public class PagePermissionViewModel
    {
        public int PageId { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class EmployeePermissionDto
    {
        public int PageId { get; set; }
        public string PageName { get; set; } = string.Empty;
        public string PageUrl { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
