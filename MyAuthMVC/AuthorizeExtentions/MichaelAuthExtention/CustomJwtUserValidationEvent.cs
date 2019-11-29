using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MyAuthMVC.Data;

namespace MyAuthMVC
{
    public class CustomJwtUserValidationEvent : JwtBearerEvents
    {
        private string UserID { get; set; }

        private string UserEmail { get; set; }

        private string UserName { get; set; }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            try
            {
                ApplicationDbContext context2 = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                //UserManager<ApplicationUser> userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();

                ClaimsPrincipal userPrincipal = context.Principal;

                this.UserID = userPrincipal.Claims.First(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                if (userPrincipal.HasClaim(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"))
                {
                    this.UserEmail = userPrincipal.Claims.First(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;
                }

                if (userPrincipal.HasClaim(c => c.Type == "name"))
                {
                    this.UserName = userPrincipal.Claims.First(c => c.Type == "name").Value;
                }

                //var checkUser = await userManager.FindByIdAsync(this.UserID);
                //if (checkUser == null)
                //{
                //    checkUser = new ApplicationUser
                //    {
                //        Id = this.UserID,
                //        Email = this.UserEmail,
                //        UserName = this.UserEmail,
                //    };

                //    var result = userManager.CreateAsync(checkUser).Result;

                //    // Assign Roles
                //    if (result.Succeeded)
                //    {
                //        return;
                //    }
                //    else
                //    {
                //        throw new Exception(result.Errors.First().Description);
                //    }
                //}
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
