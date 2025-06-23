using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Alias { get; set; }

    public int CategoryId { get; set; }

    public string? UnitDescription { get; set; }

    public double? UnitPrice { get; set; }

    public string? Image { get; set; }

    public DateTime? ManufactureDate { get; set; }

    public double? Discount { get; set; }

    public int? Views { get; set; }

    public string? Description { get; set; }

    public string SupplierId { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Referral> Referrals { get; set; } = new List<Referral>();

    public virtual Supplier Supplier { get; set; } = null!;
}
