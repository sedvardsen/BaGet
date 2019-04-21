using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace BaGet.Protocol
{
    /// <summary>
    /// The client to interact with an upstream source's Search resource.
    /// </summary>
    public class SearchClient : ISearchService
    {
        private readonly IServiceIndex _serviceIndex;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Create a new Search client.
        /// </summary>
        /// <param name="serviceIndex">The upstream package source's service index.</param>
        /// <param name="httpClient">The HTTP client used to send requests.</param>
        public SearchClient(IServiceIndex serviceIndex, HttpClient httpClient)
        {
            _serviceIndex = serviceIndex ?? throw new ArgumentNullException(nameof(serviceIndex));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <inheritdoc />
        public async Task<AutocompleteResponse> AutocompleteAsync(AutocompleteRequest request, CancellationToken cancellationToken = default)
        {
            var autocompleteUrl = await _serviceIndex.GetAutocompleteUrlAsync(cancellationToken);
            var param = (request.Type == AutocompleteRequestType.PackageIds) ? "q" : "id";
            var queryString = BuildQueryString(request, param);

            var url = QueryHelpers.AddQueryString(autocompleteUrl, queryString);

            var response = await _httpClient.DeserializeUrlAsync<AutocompleteResponse>(url, cancellationToken);

            return response.GetResultOrThrow();
        }

        /// <inheritdoc />
        public async Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
        {
            var autocompleteUrl = await _serviceIndex.GetSearchUrlAsync(cancellationToken);
            var queryString = BuildQueryString(request, "q");

            var url = QueryHelpers.AddQueryString(autocompleteUrl, queryString);

            var response = await _httpClient.DeserializeUrlAsync<SearchResponse>(url, cancellationToken);

            return response.GetResultOrThrow();
        }

        private Dictionary<string, string> BuildQueryString(SearchRequest request, string queryParamName)
        {
            var result = new Dictionary<string, string>();

            if (request.Skip != 0) result["skip"] = request.Skip.ToString();
            if (request.Take != 0) result["take"] = request.Take.ToString();
            if (request.IncludePrerelease) result["prerelease"] = true.ToString();
            if (request.IncludeSemVer2) result["semVerLevel"] = "2.0.0";

            if (!string.IsNullOrEmpty(request.Query))
            {
                result[queryParamName] = request.Query;
            }

            return result;
        }
    }
}
