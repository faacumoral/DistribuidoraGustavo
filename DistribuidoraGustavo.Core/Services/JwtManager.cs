using DistribuidoraGustavo.Interfaces.Services;
using DistribuidoraGustavo.Interfaces.Settings;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DistribuidoraGustavo.Core.Services;

public class JwtManager : IJwtManager
{
    public const string USER_ID_CLAIM = "IdUsuario";

    private readonly JwtConfiguration _jwtConfiguration;

    public JwtManager(JwtConfiguration jwtConfiguration)
    {
        _jwtConfiguration = jwtConfiguration;
    }

    public string GenerateToken(int userId)
    {
        var now = DateTime.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SecretKey));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtConfiguration.Issuer,
            Audience = _jwtConfiguration.Audience,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(USER_ID_CLAIM, userId.ToString())
            }),
            NotBefore = now,
            Expires = now.AddMinutes(_jwtConfiguration.ExpireMinutes),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}
