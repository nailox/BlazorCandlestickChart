using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorCandlestickChart;
using BlazorCandlestickChart.Pages;
using BlazorCandlestickChart.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<KlinesService>();
//builder.Services.AddSingleton<CandlestickChart>();
//builder.Services.AddSingleton<Canvas2DContext>();
//builder.Services.AddSingleton<BECanvasComponent>();
//builder.Services.AddSingleton<Candlestick>();

 await builder.Build().RunAsync();


