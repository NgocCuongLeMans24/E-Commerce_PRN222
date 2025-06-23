using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class Permission
{
    public int PermissionId { get; set; }

    public string? DepartmentId { get; set; }

    public int? PageId { get; set; }

    public bool CanAdd { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public bool CanView { get; set; }

    public virtual Department? Department { get; set; }

    public virtual WebPage? Page { get; set; }
}
