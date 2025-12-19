using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace middleware
{
    class RolesMiddlewareFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next
        )
        {
            var currentRole = context.HttpContext.Items["CurrentRole"].ToString().Split(' ')[1];
            if (currentRole.Equals("admin"))
            {
                return await next(context);
            }

            return Results.Json(
                new { error = "Неправильная роль" },
                statusCode: StatusCodes.Status405MethodNotAllowed
            );
        }
    }
}
