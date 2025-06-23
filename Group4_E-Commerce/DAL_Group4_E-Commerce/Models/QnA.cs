using System;
using System.Collections.Generic;

namespace DAL_Group4_E_Commerce.Models;

public partial class QnA
{
    public int QnAid { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public DateOnly? SubmittedDate { get; set; }

    public string EmployeeId { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
