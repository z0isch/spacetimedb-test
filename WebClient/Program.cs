using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WebClient;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebAssemblyHostBuilder.CreateDefault(args);
		builder.RootComponents.Add<App>("#app");
		builder.RootComponents.Add<HeadOutlet>("head::after");

		builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
		builder.Services.AddSingleton<ServerApi>();
		_ = Task.Run(() => StaticAssetService.GetStaticAssets(new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }));
		builder.Services.AddSingleton<StaticAssetService>();

		await builder.Build().RunAsync();
	}
}