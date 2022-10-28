using System.Net.Http.Json;

namespace BlazorCandlestickChart.Services
{
    public class KlinesService : IKlinesService
    {
        private readonly HttpClient httpClient;
        private readonly string binanceApiUrl;

        public KlinesService(HttpClient httpClient, IConfiguration config)
        {
            this.httpClient = httpClient;
            binanceApiUrl = config.GetSection("Services")["BinanceApiUrl"];
        }

        public async Task<object[][]> GetKlines()
        {
            var result = await httpClient.GetFromJsonAsync<object[][]>(binanceApiUrl) ?? Array.Empty<object[]>();
            return result;
        }
    }
}
