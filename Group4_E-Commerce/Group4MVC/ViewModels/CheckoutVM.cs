namespace GUI_Group4_ECommerce.ViewModels
{
    public class CheckoutVM
{
        public bool SameAsCustomer { get; set; }   
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
