using DistribuidoraGustavo.Interfaces.Settings;
using FMCW.Common.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DistribuidoraGustavo.API
{
    public static class Configure
    {
        public static void InitConfig(this WebApplicationBuilder builder)
        {
            ApplicationSettings.Init(new ApplicationSettings
            {
                EncryptKey = builder.Configuration["EncryptKey"]
            });
        }

        public static void AddJwtManager(this WebApplicationBuilder builder)
        {
            var audience = builder.Configuration["JWT_Audience"];
            var issuer = builder.Configuration["JWT_Issuer"];
            var secretKey = builder.Configuration["JWT_SecretKey"];

            var jwtManager = new JwtManager(new JwtConfiguration
            {
                Audience = audience,
                Issuer = issuer,
                SecretKey = secretKey,
                ExpireMinutes = 180
            });

            builder.Services.AddSingleton<IJwtManager>(jwtManager);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddJwtBearer(options =>
              {
                  options.TokenValidationParameters = new TokenValidationParameters()
                  {
                      ValidateIssuer = true,
                      ValidateAudience = true,
                      ValidateLifetime = true,
                      ValidateIssuerSigningKey = true,
                      ValidIssuer = issuer,
                      ValidAudience = audience,
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                  };
              });
        }

        public static void AddCors(this WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "Origins",
                                  builder =>
                                  {
                                      builder
                                        .AllowAnyOrigin()
                                        .AllowAnyMethod()
                                        .AllowAnyHeader();
                                  });
            });
        }
    }
}
