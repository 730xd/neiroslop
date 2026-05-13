using System;
using System.Collections.Generic;

namespace rpm1920.Models;

public partial class PriceList
{
    public int Code { get; set; }

    public decimal Price { get; set; }

    public virtual Movement CodeNavigation { get; set; } = null!;
}
