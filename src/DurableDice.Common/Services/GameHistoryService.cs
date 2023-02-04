using Azure.Data.Tables;
using DurableDice.Common.Models.History;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Services;

public class GameHistoryService
{
    private readonly TableClient _tableClient;

    public GameHistoryService(TableClient tableClient)
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

        await _tableClient.UpsertEntityAsync(history);
    }

    public async Task<GameHistory?> GetGameHistoryAsync(string gameId)
        => (await _tableClient.GetEntityAsync<GameHistory>(gameId, gameId)).Value;
}
