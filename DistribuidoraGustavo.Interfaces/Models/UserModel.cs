namespace DistribuidoraGustavo.Interfaces.Models;

public class UserModel : BaseModel
{
    public int UserId { get; set; }
    public string Username  { get; set; }
    public string Name { get; set; }
    public bool? Active { get; set; }
}

