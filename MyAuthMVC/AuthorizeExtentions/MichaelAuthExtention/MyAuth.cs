using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MyAuthMVC.Data;

namespace MyAuthMVC.AuthorizeExtentions.MichaelAuthExtention
{
    public class MyAuth
    {
    }

    public class ApiKeyAuthOpts : AuthenticationSchemeOptions
    {
        public string AuthKey { get; set; }
    }

    public class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthOpts>
    {
        public ApiKeyAuthHandler(IOptionsMonitor<ApiKeyAuthOpts> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        private const string API_TOKEN_PREFIX = "api-key";

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //// Get Authorization header value
            //if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorization))
            //{
            //    return await Task.FromResult(AuthenticateResult.Fail("Cannot read authorization header."));
            //}

            //// The auth key from Authorization header check against the configured one
            //if (authorization != Options.AuthKey)
            //{
            //    return await Task.FromResult(AuthenticateResult.Fail("Invalid auth key."));
            //}

            //// Create authenticated user
            //var identities = new List<ClaimsIdentity> { new ClaimsIdentity("custom auth type") };
            //var ticket = new AuthenticationTicket(new ClaimsPrincipal(identities), Options.Scheme);

            //return Task.FromResult(AuthenticateResult.Success(ticket));

            string token = null;
            string authorization = Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorization))
            {
                return AuthenticateResult.NoResult();
            }

            if (authorization.StartsWith(API_TOKEN_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring(API_TOKEN_PREFIX.Length).Trim();
            }

            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.NoResult();
            }

            // does the token match ?
            bool res = false;
            //using (ApplicationDbContext db = new ApplicationDbContext())
            //{
            //    //var login = db.Login.FirstOrDefault(l => l.Apikey == token);  // query db
            //    //res = login == null ? false : true;
            //}

            if (!res)
            {
                return AuthenticateResult.Fail($"token {API_TOKEN_PREFIX} not match");
            }
            else
            {
                var id = new ClaimsIdentity(
                    new Claim[] { new Claim("Key", token) },  // not safe , just as an example , should custom claims on your own
                    Scheme.Name
                );
                ClaimsPrincipal principal = new ClaimsPrincipal(id);
                var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
        }
    }
}
