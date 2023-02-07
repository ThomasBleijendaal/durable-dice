using Azure.Data.Tables;
using DurableDice.Common.Services;
using DurableDice.Functions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace DurableDice.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {

        var config = builder.GetContext().Configuration;

        var client = new TableClient(config.GetValue<string>("AzureWebJobsStorage"), "history");

        builder.Services.AddSingleton(client);
        builder.Services.AddSingleton<GameHistoryService>();
    }
}
