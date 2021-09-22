using DurableDice.Common.Models.History;
using DurableDice.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableDice.Functions.Functions
{
    public class HistoryFunction
    {
        [FunctionName(nameof(GetHistoryAsync))]
        public async Task<GameHistory?> GetHistoryAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, Route = "history/{gameId}")] HttpRequest req,
            [Table("history")] CloudTable cloudTable,
            string gameId)
                => await (new GameHistoryService(cloudTable)).GetGameHistoryAsync(gameId);
    }
}
