using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class Supplier
{
    public string SupplierId { get; set; } = null!;

    public string CompanyName { get; set; } = null!;

    public string Logo { get; set; } = null!;

    public string? ContactName { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
