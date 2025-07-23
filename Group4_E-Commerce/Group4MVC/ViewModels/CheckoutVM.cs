using System.ComponentModel.DataAnnotations;

namespace GUI_Group4_ECommerce.ViewModels
{
    public class CheckoutVM
{
        public bool SameAsCustomer { get; set; }
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Invalid phone number format (e.g., 0987654321)")]
        public string Phone { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
