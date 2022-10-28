﻿using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel;
using System.Net.WebSockets;

namespace BlazorCandlestickChart.Pages
{
    public class CandlestickChart
    {
        //  private BECanvasComponent CanvasRef { get; set; }
        private Canvas2DContext _context;

        public CandlestickChart(Canvas2DContext context, BECanvasComponent canvasRef)
        {
            _context = context;
            width = (int)canvasRef.Width;
            height = (int)canvasRef.Height;
            _context.SetFontAsync("12px sans-serif");
            _context.SetFillStyleAsync("blue");//#252525
        }

        private static double width { get; set; }
        private static double height { get; set; }

        private static string gridColor = "#444444";
        private static string gridTextColor = "#aaaaaa";
        private static string mouseHoverBackgroundColor = "#eeeeee";
        private static string mouseHoverTextColor = "#000000";
        private static string greenColor = "#00cc00";
        private static string redColor = "#cc0000";
        private static string greenHoverColor = "#00ff00";
        private static string redHoverColor = "#ff0000";

        private static double candleWidth = 5;
        private static double marginLeft = 10;
        private static double marginRight = 100;
        private static double marginTop = 10;
        private static double marginBottom = 30;

        private  double yStart = 0;
        private  double yEnd = 0;
        private static double yRange = 0;
        private static double yPixelRange = 0;

        private static double xStart = 0;
        private static double xEnd = 0;
        private static double xRange = 0;
        private static double xPixelRange = 0;

        private static double xGridCells = 16;
        private static double yGridCells = 16;

        private static bool drawMouseOverlay = false;
        private MousePosition mousePosition;
        private static double xMouseHover = 0;
        private static double yMouseHover = 0;
        private static int hoveredCandlestickID = 0;

        public List<Candlestick> candlesticks = new List<Candlestick>();

        private async Task MouseMove(MouseEventArgs e)
        {
            mousePosition.X = e.OffsetX;
            mousePosition.Y = e.OffsetY;

            mousePosition.X += candleWidth / 2;
            drawMouseOverlay = true;
            if (mousePosition.X < marginLeft) drawMouseOverlay = false;
            if (mousePosition.X > width - marginRight + candleWidth) drawMouseOverlay = false;
            if (mousePosition.Y > height - marginBottom) drawMouseOverlay= false;
            if (drawMouseOverlay)
            {
                yMouseHover = YtoValueCoords(mousePosition.Y);
                xMouseHover = XtoValueCoords(mousePosition.X);
                // snap to candlesticks
                var candlestickDelta =candlesticks.ElementAt(1).Timestamp - candlesticks.ElementAt(0).Timestamp;
                hoveredCandlestickID = (int)Math.Floor(xMouseHover - candlesticks.ElementAt(0).Timestamp / candlestickDelta);
                xMouseHover = Math.Floor(xMouseHover / candlestickDelta) * candlestickDelta;
                mousePosition.X = XtoPixelCoords(xMouseHover);
                await Draw();
            }
            else await Draw();


        }
        private async Task MouseOut(MouseEventArgs e)
        {
            drawMouseOverlay = false;
            await Draw();
        }

        public async Task Draw()
        {
            try
            {
                // clear background
                await _context.ClearRectAsync(0, 0, width, height);
                CalculateYRange();
                CalculateXRange();

                await DrawGrid();
                candleWidth = xPixelRange / candlesticks.Count;
                candleWidth--;

                if (candleWidth % 2 == 0) candleWidth--;

                for (var i = 0; i < candlesticks.Count; ++i)
                {
                    var color = (candlesticks[i].Close > candlesticks[i].Open) ? greenColor : redColor;

                    if (i == hoveredCandlestickID)
                    {
                        if (color == greenColor) color = greenHoverColor;
                        else if (color == redColor) color = redHoverColor;
                    }

                    // draw the wick
                    await DrawLine(XtoPixelCoords(candlesticks[i].Timestamp),
                                                YtoPixelCoords(candlesticks[i].Low),
                                                XtoPixelCoords(candlesticks[i].Timestamp),
                                                YtoPixelCoords(candlesticks[i].High),
                                                color);

                    // draw the candle
                    await FillRect(XtoPixelCoords(candlesticks[i].Timestamp) - Math.Floor(candleWidth / 2),
                                            YtoPixelCoords(candlesticks[i].Open), candleWidth,
                                            YtoPixelCoords(candlesticks[i].Close) - YtoPixelCoords(candlesticks[i].Open),
                                            color);

                    // draw mouse hover
                    if (drawMouseOverlay)
                    {
                        // price line
                        await _context.SetLineDashAsync(new float[] { 5.0f, 5.0f });

                        await DrawLine(0, mousePosition.Y, width, mousePosition.Y, mouseHoverBackgroundColor);
                        await _context.SetLineDashAsync(new float[] { });
                        var str = RoundPriceValue(yMouseHover).ToString();
                        var textMetrics = await _context.MeasureTextAsync(str);
                        await FillRect(width - 70, mousePosition.Y - 10, 70, 20, mouseHoverBackgroundColor);
                        await _context.SetFillStyleAsync(mouseHoverTextColor);
                        await _context.FillTextAsync(str, width - textMetrics.Width - 5, mousePosition.Y + 5);

                        // time line
                        await _context.SetLineDashAsync(new float[] { 5.0f, 5.0f });
                        await DrawLine(mousePosition.X, 0, mousePosition.X, height, mouseHoverBackgroundColor);
                        await _context.SetLineDashAsync(new float[] { });
                        str = new DateTime((int)xMouseHover).ToString();
                        textMetrics = await _context.MeasureTextAsync(str);
                        await FillRect(mousePosition.X - textMetrics.Width / 2 - 5, height - 20, textMetrics.Width + 10, 20, mouseHoverBackgroundColor);
                        await _context.SetFillStyleAsync(mouseHoverTextColor);
                        await _context.FillTextAsync(str, mousePosition.X - textMetrics.Width / 2, height - 5);

                        // data
                        var yPos = mousePosition.Y - 95;
                        if (yPos < 0) yPos = mousePosition.Y + 15;
                        await FillRect(mousePosition.X + 15, yPos, 100, 80, mouseHoverBackgroundColor);
                        var candleColor = (candlesticks.ElementAt(hoveredCandlestickID).Close > candlesticks.ElementAt(hoveredCandlestickID).Open ? greenColor : redColor);
                        await FillRect(mousePosition.X + 15, yPos, 10, 80, color);
                        await _context.SetLineWidthAsync(2);
                        await DrawRect(mousePosition.X + 15, yPos, 100, 80, candleColor);
                        await _context.SetLineWidthAsync(1);

                        await _context.SetFillStyleAsync(mouseHoverTextColor);
                        await _context.FillTextAsync("O: " + candlesticks.ElementAt(hoveredCandlestickID).Open, mousePosition.X + 30, yPos + 15);
                        await _context.FillTextAsync("C: " + candlesticks.ElementAt(hoveredCandlestickID).Close, mousePosition.X + 30, yPos + 35);
                        await _context.FillTextAsync("H: " + candlesticks.ElementAt(hoveredCandlestickID).High, mousePosition.X + 30, yPos + 55);
                        await _context.FillTextAsync("L: " + candlesticks.ElementAt(hoveredCandlestickID).Low, mousePosition.X + 30, yPos + 75);
                    }
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }

        private async Task DrawGrid()
        {
            try
            {
                // roughly divide the yRange into cells
                var yGridSize = yRange / yGridCells;

                // try to find a nice number to round to
                var niceNumber = Math.Pow(10, Math.Ceiling(Math.Log10(yGridSize)));
                if (yGridSize < 0.25 * niceNumber) niceNumber = 0.25 * niceNumber;
                else if (yGridSize < 0.5 * niceNumber) niceNumber = 0.5 * niceNumber;

                // find next largest nice number above yStart
                var yStartRoundNumber = Math.Ceiling(yStart / niceNumber) * niceNumber;

                // find next lowest nice number below yEnd
                var yEndRoundNumber = Math.Floor(yEnd / niceNumber) * niceNumber;

                for (var y = yStartRoundNumber; y <= yEndRoundNumber; y += niceNumber)
                {
                    await DrawLine(0, YtoPixelCoords(y), width, YtoPixelCoords(y), gridColor);
                    var textMetrics = _context.MeasureTextAsync(RoundPriceValue(y).ToString());// ???
                    await _context.SetFillStyleAsync(gridTextColor);
                    await _context.FillTextAsync(RoundPriceValue(y).ToString(), width - textMetrics.Result.Width - 5, YtoPixelCoords(y) - 5);
                }

                // roughly divide the xRange into cells
                var xGridSize = xRange / xGridCells;

                // try to find a nice number to round to
                niceNumber = Math.Pow(10, Math.Ceiling(Math.Log10(xGridSize)));
                if (xGridSize < 0.25 * niceNumber) niceNumber = 0.25 * niceNumber;
                else if (xGridSize < 0.5 * niceNumber) niceNumber = 0.5 * niceNumber;

                // find next largest nice number above yStart
                var xStartRoundNumber = Math.Ceiling(xStart / niceNumber) * niceNumber;
                // find next lowest nice number below yEnd
                var xEndRoundNumber = Math.Floor(xEnd / niceNumber) * niceNumber;

                // if the total x range is more than 5 days, format the timestamp as date instead of hours
                var formatAsDate = false;
                if (xRange > 60 * 60 * 24 * 1000 * 5) formatAsDate = true;
                for (var x = xStartRoundNumber; x <= xEndRoundNumber; x += niceNumber)
                {
                    await DrawLine(XtoPixelCoords(x), 0, XtoPixelCoords(x), height, gridColor);
                    //  var date = new DateTime((long)x); // ???
                    DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    DateTime date = start.AddMilliseconds(x).ToLocalTime();
                    var dateStr = string.Empty;

                    dateStr = formatAsDate ? date.ToString("dd.MM") : date.ToString("HH:mm");

                    await _context.SetFillStyleAsync(gridTextColor);
                    await _context.FillTextAsync(dateStr, XtoPixelCoords(x) + 5, height - 5);
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.StackTrace);
            }
       
        }

        private void CalculateYRange()
        {
            for (int i = 0; i < candlesticks.Count; i++)
            {
                var currentCandlestick = candlesticks.ElementAt(i);

                if (i == 0)
                {
                    yStart = currentCandlestick.Low;
                    double low = 0;
                     low = currentCandlestick.Low;
                    yEnd = currentCandlestick.High;
                }
                else
                {
                    if (currentCandlestick.Low < yStart)
                    {
                        yStart = currentCandlestick.Low;
                    }
                    if (currentCandlestick.High > yEnd)
                    {
                        yEnd = currentCandlestick.High;
                    }
                }
            }
            yRange = yEnd - yStart;
        }

        private void CalculateXRange()
        {
            xStart = candlesticks.ElementAt(0).Timestamp;
            xEnd = candlesticks.ElementAt(candlesticks.Count - 1).Timestamp;
            xRange = xEnd - xStart;
        }

        private double YtoPixelCoords(double y)
        {
            yPixelRange = height - marginTop - marginBottom;
            var coords = height - marginBottom - (y - yStart) * yPixelRange / yRange;
            return coords;

        }
        private double XtoPixelCoords(double x)
        {
            xPixelRange = width - marginLeft - marginRight;
            var coords = marginLeft + (x - xStart) * xPixelRange / xRange;
            return coords;
        }

        private double YtoValueCoords(double y)
        {
            var valCoords = yStart + (height - marginBottom - y) * yRange / yPixelRange;
            return valCoords;
        }

        private double XtoValueCoords(double x)
        {
            var valCoords = xStart + (x - marginLeft) * xRange / xPixelRange;
            return valCoords;
        }

        private async Task DrawRect(double x, double y, double width, double height, string color)
        {
            await _context.BeginPathAsync();
            await _context.SetStrokeStyleAsync(color);
            await _context.RectAsync(x, y, width, height);
            await _context.StrokeAsync();
        }

        private async Task DrawLine(double xStart, double yStart, double xEnd, double yEnd, string color)
        {
            await _context.BeginPathAsync();
            await _context.SetLineWidthAsync(1);
            await _context.MoveToAsync(xStart, yStart);
            await _context.LineToAsync(xEnd, yEnd);
            await _context.SetStrokeStyleAsync(color);
            await _context.StrokeAsync();
        }

        private double RoundPriceValue(double value)
        {
            if (value > 1.0) return Math.Round(value * 100) / 100;
            if (value > 0.001) return Math.Round(value * 1000) / 1000;
            if (value > 0.00001) return Math.Round(value * 100000) / 100000;
            if (value > 0.0000001) return Math.Round(value * 10000000) / 10000000;
            else return Math.Round(value * 1000000000) / 1000000000;
        }
        public async Task FillRect(double x, double y, double width, double height, string color)
        {
            try
            {
                await _context.BeginPathAsync();
                await _context.SetFillStyleAsync(color);
                await _context.RectAsync(x, y, width, height);
                await _context.FillAsync();
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }

    public class MousePosition
    {
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
    }
}
