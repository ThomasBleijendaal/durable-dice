using System.Security.Claims;
using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.History;
using DurableDice.Common.Models.State;
using DurableDice.Functions.Entities;
using DurableDice.Functions.Extensions;
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
            if (req.Headers.TryGetValue("x-gameid", out var gameId) &&
                req.Headers.TryGetValue("x-playerid", out var playerId))
            {
                return Negotiate(playerId, new[] { new Claim("gid", gameId) });
            }
        }
        catch
        {
        }

        return null;
    }

    [FunctionName(nameof(OnConnected))]
    public void OnConnected([SignalRTrigger] InvocationContext invocationContext, ILogger logger)
    {
        logger.LogInformation($"{invocationContext.ConnectionId} has connected");
    }

    [FunctionName(nameof(JoinGame))]
    public async Task JoinGame(
        [SignalRTrigger] InvocationContext invocationContext,
        [SignalR(HubName = nameof(GameHub))] IAsyncCollector<SignalRMessage> signalr,
        [DurableClient] IDurableEntityClient entityClient)
    {
        var gameId = invocationContext.GetGameId();

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
        AddPlayerCommand command,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(invocationContext.GetGameId()),
            x => x.AddPlayerAsync(command with { PlayerId = invocationContext.GetPlayerId() }));

    [FunctionName(nameof(Ready))]
    public async Task Ready(
        [SignalRTrigger] InvocationContext invocationContext,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(invocationContext.GetGameId()),
            x => x.ReadyAsync(invocationContext.GetPlayerId()));

    [FunctionName(nameof(AttackField))]
    public async Task AttackField(
        [SignalRTrigger] InvocationContext invocationContext,
        AttackMoveCommand command,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(invocationContext.GetGameId()),
            x => x.AttackFieldAsync(command with { PlayerId = invocationContext.GetPlayerId() }));

    [FunctionName(nameof(EndRound))]
    public async Task EndRound(
        [SignalRTrigger] InvocationContext invocationContext,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(invocationContext.GetGameId()),
            x => x.EndRoundAsync(invocationContext.GetPlayerId()));

    [FunctionName(nameof(RemovePlayer))]
    public async Task RemovePlayer(
        [SignalRTrigger] InvocationContext invocationContext,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(invocationContext.GetGameId()),
            x => x.RemovePlayerAsync(invocationContext.GetPlayerId()));

    [FunctionName(nameof(OnDisconnected))]
    public void OnDisconnected([SignalRTrigger] InvocationContext invocationContext)
    {

    }

    private static EntityId EntityId(string gameId) => new EntityId(nameof(GameEntity), gameId);
}

#pragma warning restore IDE1006 // Naming Styles
