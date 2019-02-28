using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BaGet.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BaGet.Extensions
{


    public class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private readonly IUserService _userService;

        public BasicAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserService userService)
            : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }


        //private string GetTokenFromAuthHeader()
        //{
        //    //if (!Request.Headers.TryGetValue("Authorization", out var header))
        //    //    return null;

        //    //var parts = header.FirstOrDefault()?.Split(new[] { ' ' }, 2);
        //    //if (parts?.Length != 2)
        //    //    return null;

        //    //if (parts[0] != "Bearer")
        //    //    return null;

        //    //return parts[1].Trim();
        //}
        private bool TryGetCredentialFromHeader(out NetworkCredential credential)
        {
            credential = null;
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (string.IsNullOrEmpty(authHeader.Parameter)) return false;
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentialSplit = Encoding.UTF8.GetString(credentialBytes).Split(':');
            if (credentialSplit.Length == 0)
            {
                return false;
            }

            var username = credentialSplit[0];
            var password = string.Empty;

            if (credentialSplit.Length > 1)
            {
              password  = credentialSplit[1];
            }
            credential = new NetworkCredential(username,password);
            return true;
        }


        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Headers["WWW-Authenticate"] = $"Basic realm=\"{Options.Realm}\", charset=\"UTF-8\"";
            await base.HandleChallengeAsync(properties);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            //Debug.WriteLine("DUMMY AUTH");

            //var claims = new Claim[] {
            //        //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            //        //new Claim(ClaimTypes.Name, user.Username),
            //    };
            //var identity = new ClaimsIdentity(claims, Scheme.Name);
            //var principal = new ClaimsPrincipal(identity);
            //var ticket = new AuthenticationTicket(principal, Scheme.Name);
 


            Debug.WriteLine("");
            Debug.WriteLine(nameof(HandleAuthenticateAsync));
            Debug.WriteLine(string.Format("{0}?{1} (Scheme={2})",Request.Path, Request.QueryString, Request.Scheme));
            foreach (var hk in this.Request.Headers.Keys)
            {
                var r = this.Request.Headers[hk];
                Debug.WriteLine(string.Format("{0}={1}", hk, r.ToString()));
            }

            Debug.WriteLine(string.Format("Cookies.Count={0}", this.Request.Cookies.Count));

            //return AuthenticateResult.Success(ticket);

            AuthenticationProperties prop = new AuthenticationProperties();
           
            //return AuthenticateResult.Fail("mymessage", prop)


            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.NoResult();
                // return AuthenticateResult.Fail("Missing Authorization Header");
            }

            if (TryGetCredentialFromHeader(out var credentials) == false)
            {
                return AuthenticateResult.NoResult();
                //return AuthenticateResult.Fail("Invalid Authorization Header");
            }

            if (!await _userService.Authenticate(credentials))
            {
                return AuthenticateResult.Fail("Invalid Credentials");
            }
            else
            {
                var claims = new Claim[] {
                    //new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    //new Claim(ClaimTypes.Name, user.Username),
                };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
        }
    }
}
