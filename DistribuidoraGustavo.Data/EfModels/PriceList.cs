using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class PriceList
{
    public int PriceListId { get; set; }

    public string? Name { get; set; }

    public decimal? Percent { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<ProductsPriceList> ProductsPriceLists { get; set; } = new List<ProductsPriceList>();
}
