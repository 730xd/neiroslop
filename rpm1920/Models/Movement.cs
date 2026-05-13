using System;
using System.Collections.Generic;

namespace rpm1920.Models;

public partial class Movement
{
    public int Code { get; set; }

    public string Name { get; set; } = null!;

    public int? Quantity { get; set; }

    public virtual ICollection<IssueRequest> IssueRequests { get; set; } = new List<IssueRequest>();

    public virtual PriceList? PriceList { get; set; }

    public virtual ICollection<ReceiptInvoice> ReceiptInvoices { get; set; } = new List<ReceiptInvoice>();
}
