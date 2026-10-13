using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class Family
{
    public int FamilyId { get; set; }

    public string Name { get; set; } = null!;

    public string CodeSuffix { get; set; } = null!;

    public virtual ICollection<ProductsFamily> ProductsFamilies { get; set; } = new List<ProductsFamily>();
}
