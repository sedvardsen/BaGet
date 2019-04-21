using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BaGet.Protocol
{
    /// <summary>
    /// Extensions for <see cref="IServiceIndex"/>.
    /// </summary>
    public static class IServiceIndexExtensions
    {
        // See: https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Protocol/Constants.cs
        private static readonly string Version200 = "/2.0.0";
        private static readonly string Version300beta = "/3.0.0-beta";
        private static readonly string Version300 = "/3.0.0";
        private static readonly string Version340 = "/3.4.0";
        private static readonly string Version360 = "/3.6.0";
        private static readonly string Versioned = "/Versioned";
        private static readonly string Version470 = "/4.7.0";
        private static readonly string Version490 = "/4.9.0";

        private static readonly string[] SearchQueryService = { "SearchQueryService" + Versioned, "SearchQueryService" + Version340, "SearchQueryService" + Version300beta };
        private static readonly string[] RegistrationsBaseUrl = { "RegistrationsBaseUrl" + Versioned, "RegistrationsBaseUrl" + Version360, "RegistrationsBaseUrl" + Version340, "RegistrationsBaseUrl" + Version300beta };
        private static readonly string[] SearchAutocompleteService = { "SearchAutocompleteService" + Versioned, "SearchAutocompleteService" + Version300beta };
        private static readonly string[] ReportAbuse = { "ReportAbuseUriTemplate" + Versioned, "ReportAbuseUriTemplate" + Version300 };
        private static readonly string[] LegacyGallery = { "LegacyGallery" + Versioned, "LegacyGallery" + Version200 };
        private static readonly string[] PackagePublish = { "PackagePublish" + Versioned, "PackagePublish" + Version200 };
        private static readonly string[] PackageBaseAddress = { "PackageBaseAddress" + Versioned, "PackageBaseAddress" + Version300 };
        private static readonly string[] RepositorySignatures = { "RepositorySignatures" + Version490, "RepositorySignatures" + Version470 };
        private static readonly string[] SymbolPackagePublish = { "SymbolPackagePublish" + Version490 };

        /// <summary>
        /// Get the URL for the Package Content resource.
        /// See: https://docs.microsoft.com/en-us/nuget/api/package-base-address-resource
        /// </summary>
        /// <param name="serviceIndex">The package source's service index.</param>
        /// <param name="cancellationToken">A token to cancel the task.</param>
        /// <returns>The URL for the Package Content resource.</returns>
        public static async Task<string> GetPackageContentUrlAsync(this IServiceIndex serviceIndex, CancellationToken cancellationToken = default)
        {
            return await GetUrlForResourceTypes(serviceIndex, PackageBaseAddress, cancellationToken);
        }

        /// <summary>
        /// Get the URL for the Package Metadata resource.
        /// See: https://docs.microsoft.com/en-us/nuget/api/registration-base-url-resource
        /// </summary>
        /// <param name="serviceIndex">The package source's service index.</param>
        /// <param name="cancellationToken">A token to cancel the task.</param>
        /// <returns>The URL for the Package Metadata resource.</returns>
        public static async Task<string> GetPackageMetadataUrlAsync(this IServiceIndex serviceIndex, CancellationToken cancellationToken = default)
        {
            return await GetUrlForResourceTypes(serviceIndex, RegistrationsBaseUrl, cancellationToken);

        }

        /// <summary>
        /// Get the URL for the Search resource.
        /// See: https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource
        /// </summary>
        /// <param name="serviceIndex">The package source's service index.</param>
        /// <param name="cancellationToken">A token to cancel the task.</param>
        /// <returns>The URL for the Search resource.</returns>
        public static async Task<string> GetSearchUrlAsync(this IServiceIndex serviceIndex, CancellationToken cancellationToken = default)
        {
            return await GetUrlForResourceTypes(serviceIndex, SearchQueryService, cancellationToken);
        }

        /// <summary>
        /// Get the URL for the Autocomplete resource.
        /// See: https://docs.microsoft.com/en-us/nuget/api/search-autocomplete-service-resource
        /// </summary>
        /// <param name="serviceIndex">The package source's service index.</param>
        /// <param name="cancellationToken">A token to cancel the task.</param>
        /// <returns>The URL for the Autocomplete resource.</returns>
        public static async Task<string> GetAutocompleteUrlAsync(this IServiceIndex serviceIndex, CancellationToken cancellationToken = default)
        {
            return await GetUrlForResourceTypes(serviceIndex, SearchAutocompleteService, cancellationToken);
        }

        private static async Task<string> GetUrlForResourceTypes(
            IServiceIndex serviceIndex,
            string[] types,
            CancellationToken cancellationToken)
        {
            var response = await serviceIndex.GetAsync(cancellationToken);
            var resource = types.SelectMany(t => response.Resources.Where(r => r.Type == t)).First();

            return resource.Url.Trim('/');
        }
    }
}
