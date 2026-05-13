using System;
using System.Collections.Generic;

namespace rpm1920.Models;

public partial class IssueRequest
{
    public string RequestNumber { get; set; } = null!;

    public int Code { get; set; }

    public int QuantityIssued { get; set; }

    public DateTime IssueDate { get; set; }

    public virtual Movement CodeNavigation { get; set; } = null!;
}
