using fiap.soat.fastfood.serverless.function;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication() // Use ASP.NET Core integration for Azure Functions
    .ConfigureAppConfiguration((context, builder) => { })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<JwtService>();
    })
    .Build();
host.Run();