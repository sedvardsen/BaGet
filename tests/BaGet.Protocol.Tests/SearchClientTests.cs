using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NuGet.Versioning;
using Xunit;

namespace BaGet.Protocol.Tests
{
    public class SearchClientTests
    {
        private readonly SearchClient _target;

        public SearchClientTests()
        {
            var httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            var serviceIndex = new ServiceIndexClient(httpClient, "https://api.nuget.org/v3/index.json");

            _target = new SearchClient(serviceIndex, httpClient);
        }

        [Fact]
        public async Task GetsNewtonsoftJsonSearchResults()
        {
            var registrationurl = "https://api.nuget.org/v3/registration3/newtonsoft.json/index.json";

            var result = await _target.SearchAsync(new SearchRequest { Query = "Newtonsoft" });

            Assert.True(result.TotalHits > 0);
            Assert.True(result.Data.Count > 0);
            Assert.Equal(registrationurl, result.Data[0].RegistrationUrl);
            Assert.Equal("Newtonsoft.Json", result.Data[0].Id);
            Assert.Equal(new NuGetVersion("12.0.1"), result.Data[0].Version);
        }

        [Fact]
        public async Task GetsNewtonsoftJsonAutocompleteResults()
        {
            var result = await _target.AutocompleteAsync(new AutocompleteRequest { Query = "newt" });

            Assert.True(result.TotalHits > 0);
            Assert.True(result.Data.Count > 0);
            Assert.Contains(result.Data, id => id == "Newtonsoft.Json");
        }
    }
}
