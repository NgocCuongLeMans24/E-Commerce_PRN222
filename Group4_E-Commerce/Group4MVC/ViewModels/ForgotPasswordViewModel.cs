using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        public string UserType { get; set; } = "Customer";
    }
}
