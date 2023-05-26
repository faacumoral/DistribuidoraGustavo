using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class ProductsPriceList
{
    public int ProductPriceListId { get; set; }

    public int ProductId { get; set; }

    public int PriceListId { get; set; }

    public decimal Price { get; set; }

    public virtual PriceList PriceList { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
