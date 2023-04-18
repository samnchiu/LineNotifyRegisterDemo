using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using website.DBContext;
using website.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;

namespace website.Middleware
{
    public class LineUserMiddleware
    {
        private readonly RequestDelegate _next;

        public LineUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, EFCoreContext dbContext)
        {
            string? idToken = await httpContext.GetTokenAsync("OpenIdConnect", "id_token");
            //string? access_token = await HttpContext.GetTokenAsync("OpenIdConnect", "access_token");

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(idToken);

            //(int sub, string name, string email, string idToken, string accessToken)
            var lineuser = new LineUser();
            lineuser.sub = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            lineuser.Name = token.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            lineuser.email = "";
            lineuser.IdToken = token.ToString();
            //lineuser.AccessToken = access_token;

            //不管有沒有先丟進去看看
            LineUser user = dbContext.FindOrCreateLineUserAsync(lineuser).Result;

            var sub = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(sub))
            {
                var lineUser = await dbContext.LineUsers.FirstOrDefaultAsync(u => u.sub == sub);
                if (lineUser != null)
                {
                    httpContext.Items["User"] = lineUser;
                }
            }

            await _next(httpContext);
        }
    }

    public static class LineUserMiddlewareExtensions
    {
        public static IApplicationBuilder UseLineUserMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LineUserMiddleware>();
        }
    }
}
