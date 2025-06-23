using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class Department
{
    public string DepartmentId { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public string? Info { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
