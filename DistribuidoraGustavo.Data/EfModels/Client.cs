using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class Client
{
    public int ClientId { get; set; }

    public string Name { get; set; } = null!;

    public string InvoicePrefix { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
