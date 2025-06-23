using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class WebPage
{
    public int PageId { get; set; }

    public string PageName { get; set; } = null!;

    public string Url { get; set; } = null!;

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
