    using DAL_Group4_E_Commerce.Models;
    using System.ComponentModel.DataAnnotations;

    namespace ViewModels
    {
        public class WebPageManagementViewModel
        {
            public List<WebPage> WebPages { get; set; } = new List<WebPage>();
            public List<Permission> Permissions { get; set; } = new List<Permission>();

            // Pagination
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public int TotalCount { get; set; }
            public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

            // Filters and Sorting
            public string SearchTerm { get; set; } = string.Empty;
            public string SortBy { get; set; } = "PageName";
            public string SortOrder { get; set; } = "asc";
        }

        public class CreateWebPageViewModel
        {
            [Required(ErrorMessage = "Page name is required")]
            [StringLength(50, ErrorMessage = "Page name cannot exceed 50 characters")]
            public string PageName { get; set; } = string.Empty;

            [Required(ErrorMessage = "URL is required")]
            [StringLength(250, ErrorMessage = "URL cannot exceed 250 characters")]
            public string Url { get; set; } = string.Empty;
        }

        public class EditWebPageViewModel
        {
            [Required]
            public int PageId { get; set; }

            [Required(ErrorMessage = "Page name is required")]
            [StringLength(50, ErrorMessage = "Page name cannot exceed 50 characters")]
            public string PageName { get; set; } = string.Empty;

            [Required(ErrorMessage = "URL is required")]
            [StringLength(250, ErrorMessage = "URL cannot exceed 250 characters")]
            public string Url { get; set; } = string.Empty;
        }
    }
