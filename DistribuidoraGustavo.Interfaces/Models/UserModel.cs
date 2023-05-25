namespace DistribuidoraGustavo.Interfaces.Models;

public class UserModel : BaseModel
{
    public int UserId { get; set; }
    public string Username  { get; set; } = string.Empty;
    public bool Active { get; set; }
}

