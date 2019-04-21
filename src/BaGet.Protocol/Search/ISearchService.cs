using System.Threading.Tasks;

namespace BaGet.Protocol
{
    /// <summary>
    /// The resource used to search for packages.
    /// See: https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Perform a search query.
        /// See: https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource#search-for-packages
        /// </summary>
        /// <param name="request">The search request.</param>
        /// <returns>The search response.</returns>
        Task<SearchResponse> SearchAsync(SearchRequest request);

        /// <summary>
        /// Perform an autocomplete query.
        /// See: https://docs.microsoft.com/en-us/nuget/api/search-autocomplete-service-resource
        /// </summary>
        /// <param name="request">The autocomplete request.</param>
        /// <returns>The autocomplete response.</returns>
        Task<AutocompleteResponse> AutocompleteAsync(AutocompleteRequest request);
    }
}
