using System;
using System.Collections.Generic;

namespace rpm1920.Models;

public partial class ReceiptInvoice
{
    public string InvoiceNumber { get; set; } = null!;

    public int Code { get; set; }

    public int QuantityReceived { get; set; }

    public DateTime ReceiptDate { get; set; }

    public virtual Movement CodeNavigation { get; set; } = null!;
}
