using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? Name { get; set; }

    public string? Password { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
}
