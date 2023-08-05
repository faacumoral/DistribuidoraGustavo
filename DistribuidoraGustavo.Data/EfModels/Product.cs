using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class Product
{
    public int ProductId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? Active { get; set; }

    public decimal BasePrice { get; set; }

    public int QuantityPerBox { get; set; }

    public virtual ICollection<InvoicesProduct> InvoicesProducts { get; set; } = new List<InvoicesProduct>();

    public virtual ICollection<ProductsPriceList> ProductsPriceLists { get; set; } = new List<ProductsPriceList>();
}
