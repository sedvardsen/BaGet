using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace BaGet.Protocol.Tests
{
    public class ServiceIndexClientTests
    {
        private readonly ServiceIndexClient _target;

        public ServiceIndexClientTests()
        {
            var httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });

            _target = new ServiceIndexClient(httpClient, "https://api.nuget.org/v3/index.json");
        }

        [Fact]
        public async Task GetsServiceIndex()
        {
            var result = await _target.GetAsync();

            Assert.Equal("3.0.0", result.Version.ToFullString());
        }

        [Fact]
        public async Task GetsPackageContentUrl()
        {
            var result = await _target.GetPackageContentUrlAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetsPackageMetadataUrl()
        {
            var result = await _target.GetPackageMetadataUrlAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetsSearchUrl()
        {
            var result = await _target.GetSearchUrlAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetsAutocompleteUrl()
        {
            var result = await _target.GetAutocompleteUrlAsync();

            Assert.NotNull(result);
        }
    }
}
