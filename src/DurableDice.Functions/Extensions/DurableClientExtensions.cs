using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.State;
using DurableDice.Functions.Entities;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DurableDice.Functions.Extensions;

public static class DurableClientExtensions
{
    public static async Task<string> GetNewGameIdAsync(this IDurableEntityClient durableEntityClient)
    {
        var guid = Guid.NewGuid().ToString("N");

        var i = 6;
        do
        {
            var gameId = guid[..i];
            var entityId = new EntityId(nameof(GameEntity), gameId);

            var state = await durableEntityClient.ReadEntityStateAsync<GameState>(entityId);

            if (!state.EntityExists)
            {
                await durableEntityClient.SignalEntityAsync<IGameEntity>(entityId, x => x.InitAsync());

                return gameId;
            }
        }
        while (++i < guid.Length);

        return guid;
    }
}
