using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class Customer
{
    public string CustomerId { get; set; } = null!;

    public string? Password { get; set; }

    public string FullName { get; set; } = null!;

    public bool? Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string Email { get; set; } = null!;

    public string? Photo { get; set; }

    public bool? IsActive { get; set; }

    public int? Role { get; set; }

    public string? RandomKey { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Referral> Referrals { get; set; } = new List<Referral>();
}
