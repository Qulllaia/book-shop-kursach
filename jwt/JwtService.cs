using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dtos;
using Microsoft.IdentityModel.Tokens;

namespace jwt
{
    class JwtService
    {
        public static string GenerateSimpleToken(JWTClaims userClaims)
        {
            var secretKey = Environment.GetEnvironmentVariable("SECRETKEY");
            if (secretKey == null)
            {
                throw new InvalidOperationException(
                    $"Отсутствует обязательная переменная окружения: SECRETKEY"
                );
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

            var claims = new[]
            {
                new Claim("userid_claims", userClaims.userid.ToString()),
                new Claim("email_claims", userClaims.email),
                new Claim("login", userClaims.login),
                new Claim("role", userClaims.role),
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
