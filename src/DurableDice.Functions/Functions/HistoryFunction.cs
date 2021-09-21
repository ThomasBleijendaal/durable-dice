using DurableDice.Common.Models.History;
using DurableDice.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace DurableDice.Functions.Functions
{
    public class HistoryFunction
    {
        private readonly IGameHistoryService _gameHistoryService;

        public HistoryFunction(
            IGameHistoryService gameHistoryService)
        {
            _gameHistoryService = gameHistoryService;
        }

        [FunctionName(nameof(GetHistoryAsync))]
        public async Task<GameHistory> GetHistoryAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, Route = "history/{gameId}")] HttpRequest req, 
            string gameId)
                => await _gameHistoryService.GetGameHistoryAsync(gameId);
    }
}
