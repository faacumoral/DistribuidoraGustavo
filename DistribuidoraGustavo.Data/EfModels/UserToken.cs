using System;
using System.Collections.Generic;

namespace DistribuidoraGustavo.Data.EfModels;

public partial class UserToken
{
    public int UserTokenId { get; set; }

    public int UserId { get; set; }

    public DateTime ExpireTime { get; set; }

    public string Token { get; set; } = null!;

    public string Data { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
