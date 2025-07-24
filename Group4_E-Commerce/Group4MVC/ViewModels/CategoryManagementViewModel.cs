using DAL_Group4_E_Commerce.Models;
using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class CategoryManagementViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>();

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Filters and Sorting
        public string SearchTerm { get; set; } = string.Empty;
        public string SortBy { get; set; } = "CategoryName";
        public string SortOrder { get; set; } = "asc";
    }

    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Alias cannot exceed 50 characters")]
        public string? Alias { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Image path cannot exceed 50 characters")]
        public string? Image { get; set; }
    }

    public class EditCategoryViewModel
    {
        [Required]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Alias cannot exceed 50 characters")]
        public string? Alias { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Image path cannot exceed 50 characters")]
        public string? Image { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Alias { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public int ProductCount { get; set; }
    }
}
