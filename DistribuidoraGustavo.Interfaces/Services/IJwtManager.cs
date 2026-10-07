namespace DistribuidoraGustavo.Interfaces.Services;

public interface IJwtManager
{
    string GenerateToken(int userId);
}
