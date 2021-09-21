using DurableDice.Common.Models.History;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Services
{
    public interface IGameHistoryService
    {
        Task AddGameStateAsync(string gameId, GameState state);
        Task<GameHistory?> GetGameHistoryAsync(string gameId);
    }
}
