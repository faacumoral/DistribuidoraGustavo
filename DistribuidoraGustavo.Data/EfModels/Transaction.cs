using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public int ClientId { get; set; }

    public string? Description { get; set; }

    public string Type { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;
}
