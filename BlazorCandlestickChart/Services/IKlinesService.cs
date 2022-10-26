namespace BlazorCandlestickChart.Services
{
    public interface IKlinesService
    {
        Task<object[][]> GetKlines();
    }
}
