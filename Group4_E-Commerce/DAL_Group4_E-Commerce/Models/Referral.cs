using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class Referral
{
    public int ReferralId { get; set; }

    public string? CustomerId { get; set; }

    public int ProductId { get; set; }

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public DateTime? SentDate { get; set; }

    public string? Note { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Product Product { get; set; } = null!;
}
