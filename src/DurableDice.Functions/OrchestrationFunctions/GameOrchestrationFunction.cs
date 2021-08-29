using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;
using DurableDice.Data.Entities;
using DurableDice.Functions.SignalRFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace DurableDice.Functions.OrchestrationFunctions;

public class GameOrchestrationFunction
{
    [FunctionName(nameof(AddPlayerAsync))]
    public async Task AddPlayerAsync(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        [DurableClient] IDurableEntityClient entityClient,
        [SignalR(HubName = nameof(GameHub))] IAsyncCollector<SignalRMessage> signalr)
    {
        var input = context.GetInput<AddPlayerCommand>();

        var gameId = new EntityId(nameof(GameEntity), input.GameId);

        bool accepted;
        using (await context.LockAsync(gameId))
        {
            var game = context.CreateEntityProxy<IGameEntity>(gameId);
            accepted = await game.AddPlayerAsync(input);
        }

        if (accepted)
        {
            await SendGameStateUpdateAsync(entityClient, signalr, gameId, input.GameId);
        }
    }

    [FunctionName(nameof(StartMatchAsync))]
    public async Task StartMatchAsync(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        [DurableClient] IDurableEntityClient entityClient,
        [SignalR(HubName = nameof(GameHub))] IAsyncCollector<SignalRMessage> signalr)
    {
        var input = context.GetInput<StartMatchCommand>();

        var gameId = new EntityId(nameof(GameEntity), input.GameId);

        bool accepted;
        using (await context.LockAsync(gameId))
        {
            var game = context.CreateEntityProxy<IGameEntity>(gameId);
            accepted = await game.StartMatchAsync();
        }

        if (accepted)
        {
            await SendGameStateUpdateAsync(entityClient, signalr, gameId, input.GameId);
        }
    }

    // TODO: get gameId from entity
    private static async Task SendGameStateUpdateAsync(IDurableEntityClient entityClient, IAsyncCollector<SignalRMessage> signalr, EntityId gameEntityId, string gameId)
    {
        var gameStateResponse = await entityClient.ReadEntityStateAsync<GameState>(gameEntityId);

        if (gameStateResponse.EntityExists)
        {
            await signalr.AddAsync(new SignalRMessage
            {
                GroupName = gameId,
                Arguments = new object[] { gameStateResponse.EntityState },
                Target = "Broadcast"
            });
        }
    }
}
