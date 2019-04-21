using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BaGet.Protocol
{
    public class ServiceIndexClient : IServiceIndex
    {
        private readonly HttpClient _httpClient;
        private readonly string _indexUrl;

        public ServiceIndexClient(HttpClient httpClient, string indexUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _indexUrl = indexUrl ?? throw new ArgumentNullException(nameof(indexUrl));
        }

        public async Task<ServiceIndexResponse> GetAsync()
        {
            var response = await _httpClient.DeserializeUrlAsync<ServiceIndexResponse>(_indexUrl);

            return response.GetResultOrThrow();
        }
    }
}
