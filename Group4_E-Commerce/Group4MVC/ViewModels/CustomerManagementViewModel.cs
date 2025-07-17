using DAL_Group4_E_Commerce.Models;
using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class CustomerManagementViewModel
    {
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public string SearchTerm { get; set; } = string.Empty;
        public string StatusFilter { get; set; } = "All"; // All, Active, Inactive
        public string SortBy { get; set; } = "FullName"; // FullName, Email, BirthDate
        public string SortOrder { get; set; } = "asc"; // asc, desc
    }

    public class CreateCustomerViewModel
    {
        [Required(ErrorMessage = "Customer ID is required")]
        [StringLength(20, ErrorMessage = "Customer ID cannot exceed 20 characters")]
        public string CustomerID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(50, ErrorMessage = "Full name cannot exceed 50 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [StringLength(24, ErrorMessage = "Phone cannot exceed 24 characters")]
        public string? Phone { get; set; }

        [StringLength(60, ErrorMessage = "Address cannot exceed 60 characters")]
        public string? Address { get; set; }

        public bool Gender { get; set; } = false;

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; } = DateTime.Now.AddYears(-18);

        public bool IsActive { get; set; } = true;
    }

    public class EditCustomerViewModel
    {
        [Required]
        public string CustomerID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(50, ErrorMessage = "Full name cannot exceed 50 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(50, ErrorMessage = "Email cannot exceed 50 characters")]
        public string Email { get; set; } = string.Empty;

        [StringLength(24, ErrorMessage = "Phone cannot exceed 24 characters")]
        public string? Phone { get; set; }

        [StringLength(60, ErrorMessage = "Address cannot exceed 60 characters")]
        public string? Address { get; set; }

        public bool Gender { get; set; } = false;

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public bool IsActive { get; set; }

        [StringLength(50)]
        public string Photo { get; set; } = "Photo.gif";
    }
}
