using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using BaGet.Core.Services;
using LazyCache;

namespace BaGet.Extensions
{

    /// <summary>
    /// we call it "NugetAuthenticationHandler" because the implementaion is specially for the Nuget Client and Nuget Client is ignoring some of the RFC's
    /// </summary>
    public class AzureDevopsAuthenticationHandler : AuthenticationHandler<NugetAuthenticationOptions>
    {
        private readonly ICredentialsValidationService _authenticationService;
        private readonly IAppCache _appCache;

        public AzureDevopsAuthenticationHandler(
            IOptionsMonitor<NugetAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            ICredentialsValidationService authenticationService,
            IAppCache appCache)
            : base(options, logger, encoder, clock)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _appCache = appCache;
        }

        private static bool TryGetCredentialFromHeader(string authHeaderString, out NetworkCredential credential)
        {
            credential = null;
            var authHeader = AuthenticationHeaderValue.Parse(authHeaderString);
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

        private static readonly string Challenge = "Basic"; //03/2019 => Nuget Client supports "Basic" only. Token scenarios are handled as "Basic" not as "Bearer"

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var authResult = await HandleAuthenticateOnceSafeAsync();
            if (authResult == null) return;

            if (authResult.Succeeded==false)
            {
                Response.Headers.Append(HeaderNames.WWWAuthenticate, $"{Challenge}"); //realm, charset, extended error info ?
            }
            await base.HandleChallengeAsync(properties);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authorization = Request.Headers["Authorization"];
            var httpMethod = Request.Method;

            // If no authorization header found, nothing to process further
            if (string.IsNullOrEmpty(authorization))
            {
                Logger.LogTrace("Request Header does not contains 'Authorization'");
                return AuthenticateResult.NoResult();
            }


            if (TryGetCredentialFromHeader(authorization, out var credentials) == false)
            {
                Logger.LogTrace("Request Header contains 'Authorization' but it is not valid for Nuget");
                return AuthenticateResult.NoResult();
            }
            var cred = credentials.GetCredential(new Uri("http://tempuri.org"), "Basic");

            // Using nullable to force retry on false
            var isValid = await _appCache.GetOrAddAsync(cred.Password, async () => await _authenticationService.IsValid(credentials, httpMethod)? (bool?)true:null, DateTimeOffset.Now.AddMinutes(5));
            if (isValid != true)
            {
                Logger.LogTrace("Access denied by AuthenticationService");
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
