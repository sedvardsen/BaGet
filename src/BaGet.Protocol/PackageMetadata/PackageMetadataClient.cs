using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;

namespace BaGet.Protocol
{
    /// <summary>
    /// The client to interact with an upstream source's Package Metadata resource.
    /// </summary>
    public class PackageMetadataClient : IPackageMetadataService
    {
        private readonly IServiceIndex _serviceIndex;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Create a new Package Metadata client.
        /// </summary>
        /// <param name="serviceIndex">The upstream package source's service index.</param>
        /// <param name="httpClient">The HTTP client used to send requests.</param>
        public PackageMetadataClient(IServiceIndex serviceIndex, HttpClient httpClient)
        {
            _serviceIndex = serviceIndex ?? throw new ArgumentNullException(nameof(serviceIndex));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <inheritdoc />
        public async Task<RegistrationIndexResponse> GetRegistrationIndexOrNullAsync(string id, CancellationToken cancellationToken = default)
        {
            var packageMetadataUrl = await _serviceIndex.GetPackageMetadataUrlAsync(cancellationToken);
            var packageId = id.ToLowerInvariant();

            var url = $"{packageMetadataUrl}/{packageId}/index.json";

            var response = await _httpClient.DeserializeUrlAsync<RegistrationIndexResponse>(url, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return response.GetResultOrThrow();
        }

        /// <inheritdoc />
        public async Task<RegistrationPageResponse> GetRegistrationPageOrNullAsync(
            string id,
            NuGetVersion lower,
            NuGetVersion upper,
            CancellationToken cancellationToken = default)
        {
            var packageMetadataUrl = await _serviceIndex.GetPackageMetadataUrlAsync(cancellationToken);
            var packageId = id.ToLowerInvariant();
            var lowerVersion = lower.ToNormalizedString().ToLowerInvariant();
            var upperVersion = upper.ToNormalizedString().ToLowerInvariant();

            var url = $"{packageMetadataUrl}/{packageId}/page/{lowerVersion}/{upperVersion}.json";

            var response = await _httpClient.DeserializeUrlAsync<RegistrationPageResponse>(url, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return response.GetResultOrThrow();
        }

        /// <inheritdoc />
        public async Task<RegistrationLeafResponse> GetRegistrationLeafOrNullAsync(
            string id,
            NuGetVersion version,
            CancellationToken cancellationToken = default)
        {
            var packageMetadataUrl = await _serviceIndex.GetPackageMetadataUrlAsync(cancellationToken);
            var packageId = id.ToLowerInvariant();
            var packageVersion = version.ToNormalizedString().ToLowerInvariant();

            var url = $"{packageMetadataUrl}/{packageId}/{packageVersion}.json";

            var response = await _httpClient.DeserializeUrlAsync<RegistrationLeafResponse>(url, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return response.GetResultOrThrow();
        }
    }
}
