using System;
using Microsoft.Extensions.Options;

namespace BaGet.Extensions
{
    public class NugetAuthenticationPostConfigureOptions : IPostConfigureOptions<NugetAuthenticationOptions>
    {
        public void PostConfigure(string name, NugetAuthenticationOptions options)
        {
            //if (string.IsNullOrEmpty(options.Realm))
            //{
            //    throw new InvalidOperationException("Realm must be provided in options");
            //}
        }
    }
}
