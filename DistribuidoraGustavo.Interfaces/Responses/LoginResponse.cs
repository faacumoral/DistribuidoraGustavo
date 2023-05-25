using DistribuidoraGustavo.Interfaces.Models;

namespace DistribuidoraGustavo.Interfaces.Responses;

public class LoginResponse : Response
{
    public string Jwt { get; set; }
    public UserModel User { get; set; }
}
