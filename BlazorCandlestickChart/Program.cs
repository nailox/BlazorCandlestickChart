using BlazorCandlestickChart;
using BlazorCandlestickChart.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<KlinesService>();

 await builder.Build().RunAsync();

//var host = builder.Build();

//var klinesService = host.Services.GetRequiredService<KlinesService>();
//await weatherService.InitializeWeatherAsync(
//    host.Configuration["WeatherServiceUrl"]);


