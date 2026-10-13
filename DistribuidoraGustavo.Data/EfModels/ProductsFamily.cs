using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class ProductsFamily
{
    public int ProductFamilyId { get; set; }

    public int ProductId { get; set; }

    public int FamilyId { get; set; }

    public virtual Family Family { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
