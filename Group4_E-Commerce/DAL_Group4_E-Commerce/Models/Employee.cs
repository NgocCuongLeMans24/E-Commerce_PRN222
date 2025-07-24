using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class Employee
{
    public string EmployeeId { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public virtual ICollection<FeedbackTopic> FeedbackTopics { get; set; } = new List<FeedbackTopic>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    public virtual ICollection<QnA> QnAs { get; set; } = new List<QnA>();
}
