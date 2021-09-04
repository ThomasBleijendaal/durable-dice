using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;
using DurableDice.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace DurableDice.Functions.SignalRFunctions;

#pragma warning disable IDE1006 // Naming Styles

public class GameHub : ServerlessHub
{
    [FunctionName(nameof(Negotiate))]
    public SignalRConnectionInfo? Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req)
    {
        try
        {
            // TODO: setup secret for authenticating requests
            return Negotiate(
                req.Headers.TryGetValue("x-ms-signalr-user-id", out var value) ? value : "");
        }
        catch
        {
            return null;
        }
    }

    [FunctionName(nameof(OnConnected))]
    public void OnConnected([SignalRTrigger] InvocationContext invocationContext, ILogger logger)
    {
        logger.LogInformation($"{invocationContext.ConnectionId} has connected");
    }

    [FunctionName(nameof(JoinGame))]
    public async Task JoinGame(
        [SignalRTrigger] InvocationContext invocationContext,
        string gameId,
        [SignalR(HubName = nameof(GameHub))] IAsyncCollector<SignalRMessage> signalr,
        [DurableClient]IDurableEntityClient entityClient)
    {
        await Groups.AddToGroupAsync(invocationContext.ConnectionId, gameId);

        var entity = await entityClient.ReadEntityStateAsync<GameState>(new EntityId(nameof(GameEntity), gameId));
        if (entity.EntityExists)
        {
            await signalr.AddAsync(new SignalRMessage
            {
                GroupName = gameId,
                Arguments = new object[] { entity.EntityState },
                Target = "Broadcast"
            });
        }
    }

    [FunctionName(nameof(AddPlayer))]
    public async Task AddPlayer(
        [SignalRTrigger] InvocationContext invocationContext,
        string gameId,
        AddPlayerCommand command,
        [DurableClient] IDurableClient durableClient) 
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(gameId),
            x => x.AddPlayerAsync(command));

    [FunctionName(nameof(Ready))]
    public async Task Ready(
        [SignalRTrigger] InvocationContext invocationContext,
        string gameId,
        string playerId,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(gameId),
            x => x.ReadyAsync(playerId));

    [FunctionName(nameof(AttackField))]
    public async Task AttackField(
        [SignalRTrigger] InvocationContext invocationContext,
        string gameId,
        AttackMoveCommand command,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(gameId),
            x => x.AttackFieldAsync(command));

    [FunctionName(nameof(EndRound))]
    public async Task EndRound(
        [SignalRTrigger] InvocationContext invocationContext,
        string gameId,
        string playerId,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(gameId),
            x => x.EndRoundAsync(playerId));

    [FunctionName(nameof(RemovePlayer))]
    public async Task RemovePlayer(
        [SignalRTrigger] InvocationContext invocationContext,
        string gameId,
        string playerId,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(gameId),
            x => x.RemovePlayerAsync(playerId));

    [FunctionName(nameof(OnDisconnected))]
    public void OnDisconnected([SignalRTrigger] InvocationContext invocationContext)
    {

    }

    private static EntityId EntityId(string gameId) => new EntityId(nameof(GameEntity), gameId);
}

#pragma warning restore IDE1006 // Naming Styles
