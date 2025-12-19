using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace middleware
{
    class AuthMiddlewareFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next
        )
        {
            var token = context
                .HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()
                ?.Split(" ")
                .Last();

            if (string.IsNullOrEmpty(token))
            {
                return Results.Json(
                    new { error = "Требуется авторизация" },
                    statusCode: StatusCodes.Status401Unauthorized
                );
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRETKEY")!);

                var jwtToken = tokenHandler.ReadJwtToken(token);

                var claims = jwtToken.Claims.ToList();

                context.HttpContext.Items["CurrentRole"] = claims.FirstOrDefault(c =>
                    c.Type == "role"
                );

                context.HttpContext.Items["CurrentId"] = claims.FirstOrDefault(c =>
                    c.Type == "userid_claims"
                );

                tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                    },
                    out _
                );
            }
            catch (SecurityTokenExpiredException)
            {
                return Results.Json(
                    new { error = "Токен истек" },
                    statusCode: StatusCodes.Status401Unauthorized
                );
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Results.Json(
                    new { error = "Невалидный токен" },
                    statusCode: StatusCodes.Status401Unauthorized
                );
            }
            return await next(context);
        }
    }
}
