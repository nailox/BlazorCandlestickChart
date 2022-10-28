namespace BlazorCandlestickChart.Pages
{
    public class Candlestick
    {
        public Candlestick(long timestamp, double open, double close, double high, double low)
        {
            Timestamp = timestamp;
            High = high;
            Open = open;
            Close = close;
            Low = low;
        }

        public long Timestamp { get; set; }
        public double High { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Low { get; set; }
    }
}
