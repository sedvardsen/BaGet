using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;

namespace BaGet.Protocol
{
    /// <summary>
    /// The client to interact with an upstream source's Package Content resource.
    /// </summary>
    public class PackageContentClient : IPackageContentService
    {
        private readonly IServiceIndex _serviceIndex;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Create a new Package Content client.
        /// </summary>
        /// <param name="serviceIndex">The upstream package source's service index.</param>
        /// <param name="httpClient">The HTTP client used to send requests.</param>
        public PackageContentClient(IServiceIndex serviceIndex, HttpClient httpClient)
        {
            _serviceIndex = serviceIndex ?? throw new ArgumentNullException(nameof(serviceIndex));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <inheritdoc />
        public async Task<PackageVersionsResponse> GetPackageVersionsOrNullAsync(string id, CancellationToken cancellationToken = default)
        {
            var packageContentUrl = await _serviceIndex.GetPackageContentUrlAsync(cancellationToken);
            var packageId = id.ToLowerInvariant();

            var url = $"{packageContentUrl}/{packageId}/index.json";

            var response = await _httpClient.DeserializeUrlAsync<PackageVersionsResponse>(url, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return response.GetResultOrThrow();
        }

        /// <inheritdoc />
        public async Task<Stream> GetPackageContentStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken = default)
        {
            var packageContentUrl = await _serviceIndex.GetPackageContentUrlAsync(cancellationToken);
            var packageId = id.ToLowerInvariant();
            var packageVersion = version.ToNormalizedString().ToLowerInvariant();

            var url = $"{packageContentUrl}/{packageId}/{packageVersion}/{packageId}.{packageVersion}.nupkg";

            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return await response.Content.ReadAsStreamAsync();
        }

        /// <inheritdoc />
        public async Task<Stream> GetPackageManifestStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken = default)
        {
            var packageContentUrl = await _serviceIndex.GetPackageContentUrlAsync(cancellationToken);
            var packageId = id.ToLowerInvariant();
            var packageVersion = version.ToNormalizedString().ToLowerInvariant();

            var url = $"{packageContentUrl}/{packageId}/{packageVersion}/{packageId}.nuspec";

            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return await response.Content.ReadAsStreamAsync();
        }
    }
}
