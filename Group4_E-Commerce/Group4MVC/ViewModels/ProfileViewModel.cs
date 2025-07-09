using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;

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

        // Customer-specific fields
        public bool? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Photo { get; set; }
        public bool? IsActive { get; set; }

        // Password change fields - these will be populated from the change password form
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }

        // Photo upload
        public IFormFile? ProfilePhoto { get; set; }
        public string? CurrentPhotoUrl { get; set; }
    }
}
