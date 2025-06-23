using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class Favorite
{
    public int FavoriteId { get; set; }

    public int? ProductId { get; set; }

    public string? CustomerId { get; set; }

    public DateTime? SelectedDate { get; set; }

    public string? Note { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Product? Product { get; set; }
}
