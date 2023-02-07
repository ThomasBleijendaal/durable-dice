using DurableDice.Functions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableDice.Functions.Functions;

public class GameFunction
{
    [FunctionName(nameof(GetGameIdAsync))]
    public async Task<string> GetGameIdAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "game/new")] HttpRequest req,
        [DurableClient] IDurableEntityClient durableEntityClient)
        => await durableEntityClient.GetNewGameIdAsync();
}
