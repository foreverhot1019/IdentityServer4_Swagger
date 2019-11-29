using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using MyAuthMVC.Data;

public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly ApplicationDbContext _AppDbContext;

    public CustomCookieAuthenticationEvents(ApplicationDbContext AppDbContext)
    {
        // Get the database from registered DI services.
        _AppDbContext = AppDbContext;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (context == null) throw new System.ArgumentNullException(nameof(context));

        var userPrincipal = context.Principal;
        var userId = userPrincipal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            context.RejectPrincipal();
            return;
        }

        // Get an instance using DI
        var dbContext = _AppDbContext;// context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            context.RejectPrincipal();
            return;
        }
        else
        {
            //修改过用户信息或者主动让账户过期（修改SecurityStamp）
            var SecurityStamp = userPrincipal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.AuthenticationInstant)?.Value;
            
            if (user.SecurityStamp != SecurityStamp)
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }
        }
    }
}
