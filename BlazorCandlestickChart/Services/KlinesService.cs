using System.Net.Http.Json;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorCandlestickChart.Pages;

namespace BlazorCandlestickChart.Services
{
    public class KlinesService : IKlinesService
    {
        private readonly HttpClient httpClient;
        private readonly string binanceApiUrl;
        private Canvas2DContext _context;
        BECanvasComponent canvasRef;

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
