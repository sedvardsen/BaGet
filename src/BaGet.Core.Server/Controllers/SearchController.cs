using System;
using System.Threading.Tasks;
using BaGet.Core.Search;
using BaGet.Extensions;
using BaGet.Protocol;
using Microsoft.AspNetCore.Mvc;

namespace BaGet.Controllers
{
    public class SearchController : Controller
    {
        private readonly IBaGetSearchService _searchService;

        public SearchController(IBaGetSearchService searchService)
        {
            _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        }

        public async Task<ActionResult<SearchResponse>> Get(
            [FromQuery(Name = "q")] string query = null,
            [FromQuery]int skip = 0,
            [FromQuery]int take = 20,
            [FromQuery]bool prerelease = false,
            [FromQuery]string semVerLevel = null,

            // These are unofficial parameters
            [FromQuery]string packageType = null,
            [FromQuery]string framework = null)
        {
            var includeSemVer2 = semVerLevel == "2.0.0";
            var results = await _searchService.SearchAsync(new BaGetSearchRequest
            {
                Skip = skip,
                Take = take,
                IncludePrerelease = prerelease,
                IncludeSemVer2 = includeSemVer2,
                Query = query ?? string.Empty,

                PackageType = packageType,
                Framework = framework,
            });

            return new SearchResponse(
                totalHits: results.TotalHits,
                data: results.Data,
                context: SearchContext.Default(Url.RegistrationsBase()));
        }

        public async Task<ActionResult<AutocompleteResponse>> Autocomplete([FromQuery(Name = "q")] string query = null)
        {
            // TODO: Add other autocomplete parameters
            // TODO: Support versions autocomplete.
            return await _searchService.AutocompleteAsync(new AutocompleteRequest
            {
                Skip = 0,
                Take = 20,
                Query = query
            });
        }

        public async Task<ActionResult<DependentsResponse>> Dependents([FromQuery] string packageId)
        {
            // TODO: Add other dependents parameters.
            return await _searchService.FindDependentsAsync(new DependentsRequest
            {
                PackageId = packageId,
            });
        }
    }
}
