using DAL_Group4_E_Commerce.Models;
using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class SupplierManagementViewModel
    {
        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; } = "";
        public string SortBy { get; set; } = "CompanyName";
        public string SortOrder { get; set; } = "asc";

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class CreateSupplierViewModel
    {
        [Required(ErrorMessage = "Supplier ID is required")]
        [StringLength(20, ErrorMessage = "Supplier ID cannot exceed 20 characters")]
        public string SupplierId { get; set; } = "";

        [Required(ErrorMessage = "Company Name is required")]
        [StringLength(100, ErrorMessage = "Company Name cannot exceed 100 characters")]
        public string CompanyName { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = "";

        [StringLength(100, ErrorMessage = "Contact Name cannot exceed 100 characters")]
        public string? ContactName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(200, ErrorMessage = "Logo URL cannot exceed 200 characters")]
        public string Logo { get; set; } = "/placeholder.svg?height=50&width=50";

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class EditSupplierViewModel
    {
        [Required]
        public string SupplierId { get; set; } = "";

        [Required(ErrorMessage = "Company Name is required")]
        [StringLength(100, ErrorMessage = "Company Name cannot exceed 100 characters")]
        public string CompanyName { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = "";

        [StringLength(100, ErrorMessage = "Contact Name cannot exceed 100 characters")]
        public string? ContactName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(200, ErrorMessage = "Logo URL cannot exceed 200 characters")]
        public string Logo { get; set; } = "/placeholder.svg?height=50&width=50";
    }
}
