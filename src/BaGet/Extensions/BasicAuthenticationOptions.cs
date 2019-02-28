using Microsoft.AspNetCore.Authentication;

namespace BaGet.Extensions
{
    //https://joonasw.net/view/creating-auth-scheme-in-aspnet-core-2




    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string Realm { get; set; } = "defaultrealm";
    }
}
