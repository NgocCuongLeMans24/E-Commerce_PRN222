using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public string CustomerId { get; set; } = null!;

    public DateTime? OrderDate { get; set; }

    public DateTime? RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public string? CustomerName { get; set; }

    public string Address { get; set; } = null!;

    public string? PaymentMethod { get; set; }

    public string? ShippingMethod { get; set; }

    public double? Freight { get; set; }

    public int? StatusId { get; set; }

    public string? EmployeeId { get; set; }

    public string? Note { get; set; }

    public string? PhoneNumber { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Status? Status { get; set; }
}
