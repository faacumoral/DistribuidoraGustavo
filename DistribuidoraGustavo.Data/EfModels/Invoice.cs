using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public int ClientId { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool Active { get; set; }

    public int? PriceListId { get; set; }

    public string? Description { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<InvoicesProduct> InvoicesProducts { get; set; } = new List<InvoicesProduct>();

    public virtual PriceList? PriceList { get; set; }
}
