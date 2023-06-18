using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class InvoicesProduct
{
    public int InvoicesProductsId { get; set; }

    public int ProductId { get; set; }

    public int InvoiceId { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal Amount { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
