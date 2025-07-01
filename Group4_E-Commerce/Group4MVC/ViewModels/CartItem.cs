namespace GUI_Group4_ECommerce.ViewModels   
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string? Image { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }

        public double Total => UnitPrice * Quantity;
    }
}
