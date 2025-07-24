using DAL_Group4_E_Commerce.Models;
using ViewModels;

namespace Group4MVC.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string employeeId, string pageUrl, string action);
        Task<List<string>> GetAccessiblePageUrlsAsync(string employeeId);
        Task<Dictionary<string, Dictionary<string, bool>>> GetEmployeePermissionsAsync(string employeeId);
        Task<List<Permission>> GetEmployeePermissionsForPageAsync(string employeeId, int pageId);
        Task<bool> UpdateEmployeePermissionsAsync(string employeeId, Dictionary<int, PagePermissionViewModel> permissions);
        Task<WebPage> GetPageByUrlAsync(string url);
    }

    //public class PagePermissionViewModel
    //{
    //    public int PageId { get; set; }
    //    public bool CanView { get; set; }
    //    public bool CanAdd { get; set; }
    //    public bool CanEdit { get; set; }
    //    public bool CanDelete { get; set; }
    //}
}
