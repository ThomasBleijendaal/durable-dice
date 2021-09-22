using DurableDice.Common.Models.History;
using DurableDice.Common.Models.State;
using Microsoft.Azure.Cosmos.Table;

namespace DurableDice.Common.Services
{
    public class GameHistoryService
    {
        private readonly CloudTable _tableClient;

        public GameHistoryService(CloudTable tableClient)
        {
            _tableClient = tableClient;
        }

        public async Task AddGameStateAsync(string gameId, GameState state)
        {
            var history = await GetGameHistoryAsync(gameId);
            if (history == null)
            {
                history = new GameHistory(gameId, state);
            }
            else
            {
                history.AddState(state);
            }

            var updateOperation = TableOperation.InsertOrReplace(history);

            await _tableClient.ExecuteAsync(updateOperation);
        }

        public async Task<GameHistory?> GetGameHistoryAsync(string gameId)
            => (await _tableClient.ExecuteAsync(TableOperation.Retrieve<GameHistory>(gameId, gameId))).Result as GameHistory;
    }
}
