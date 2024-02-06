using System.Security.Claims;
using DurableDice.Common.Abstractions;
using DurableDice.Common.Enums;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;
using DurableDice.Functions.Entities;
using DurableDice.Functions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DurableDice.Functions.SignalRFunctions;

#pragma warning disable IDE1006 // Naming Styles

public class GameHub : ServerlessHub
{
    [FunctionName(nameof(Negotiate))]
    public SignalRConnectionInfo? Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req)
    {
        try
        {
            return Negotiate(Guid.NewGuid().ToString(), new[] { new Claim("gid", Guid.NewGuid().ToString()) });

            if (req.Headers.TryGetValue("x-gameid", out var gameId) &&
                req.Headers.TryGetValue("x-playerid", out var playerId))
            {
                return Negotiate(playerId, new[] { new Claim("gid", gameId) });
            }
        }
        catch
        {
            // don't care
        }

        return null;
    }

    [FunctionName(nameof(OnConnected))]
    public void OnConnected([SignalRTrigger] InvocationContext invocationContext, ILogger logger)
    {
        logger.LogInformation($"{invocationContext.ConnectionId} has connected");
    }

    // F# frontend pushes to same method
    [FunctionName(nameof(Send))]
    public async Task Send(
        [SignalRTrigger] InvocationContext invocationContext,
        [SignalR(HubName = nameof(GameHub))] IAsyncCollector<SignalRMessage> signalr,
        [DurableClient] IDurableClient durableClient)
    {
        if (invocationContext.Arguments[0] is string command && command == "JoinGame")
        {
            await JoinGame(invocationContext, signalr, durableClient);
        }
        else if (invocationContext.Arguments[0] is JArray commandParts)
        {
            var result = commandParts.Values<string>().OfType<string>().ToArray() switch
            {
                { Length: 3 } addBot when addBot[0].Equals("AddBot") => AddBot(invocationContext, new(invocationContext.GetPlayerId(), Enum.Parse<BotType>(addBot[2])), durableClient),
                { Length: 3 } addPlayer when addPlayer[0].Equals("AddPlayer") => AddPlayer(invocationContext, new(invocationContext.GetPlayerId(), addPlayer[2]), durableClient),
                { Length: 2 } removePlayer when removePlayer[0].Equals("RemovePlayer") => RemovePlayer(invocationContext, durableClient),
                { Length: 4 } moveField when moveField[0].Equals("MoveField") => MoveField(invocationContext, new(invocationContext.GetPlayerId(), moveField[2], moveField[3]), durableClient),
                { Length: 2 } endRound when endRound[0].Equals("EndRound") => EndRound(invocationContext, durableClient),
                { Length: 1 } ready when ready[0].Equals("Ready") => Ready(invocationContext, durableClient),
                // TODO: parse rules
                { Length: 2 } ready when ready[0].Equals("ReadyWithRules") => ReadyWithRules(invocationContext, new(invocationContext.GetPlayerId(), new()), durableClient),

                _ => Task.CompletedTask
            };

            await result;
        }

    }

    [FunctionName(nameof(JoinGame))]
    public async Task JoinGame(
        [SignalRTrigger] InvocationContext invocationContext,
        [SignalR(HubName = nameof(GameHub))] IAsyncCollector<SignalRMessage> signalr,
        [DurableClient] IDurableEntityClient entityClient)
    {
        var gameId = invocationContext.GetGameId();

        await Groups.AddToGroupAsync(invocationContext.ConnectionId, gameId);

        var entity = await entityClient.ReadEntityStateAsync<GameState>(EntityId(gameId));
        if (entity.EntityExists)
        {
            await signalr.AddAsync(new SignalRMessage
            {
                GroupName = gameId,
                Arguments = new object[] { new { connectionId = Guid.NewGuid(), invocationId = Guid.NewGuid(), message = new object[] { this } } },
                Target = "Send"
            });
        }
    }

    [FunctionName(nameof(AddBot))]
    public async Task AddBot(
        [SignalRTrigger] InvocationContext invocationContext,
        AddBotCommand command,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(invocationContext.GetGameId()),
            x => x.AddBotAsync(command with { PlayerId = invocationContext.GetPlayerId() }));

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

    [FunctionName(nameof(ReadyWithRules))]
    public async Task ReadyWithRules(
        [SignalRTrigger] InvocationContext invocationContext,
        ReadyPlayerCommand command,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(invocationContext.GetGameId()),
            x => x.ReadyWithRulesAsync(command with { PlayerId = invocationContext.GetPlayerId() }));

    [FunctionName(nameof(MoveField))]
    public async Task MoveField(
        [SignalRTrigger] InvocationContext invocationContext,
        MoveCommand command,
        [DurableClient] IDurableClient durableClient)
        => await durableClient.SignalEntityAsync<IGameEntity>(
            EntityId(invocationContext.GetGameId()),
            x => x.MoveFieldAsync(command with { PlayerId = invocationContext.GetPlayerId() }));

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
        // do nothing
    }

    private static EntityId EntityId(string gameId) => new EntityId(nameof(GameEntity), gameId);
}

#pragma warning restore IDE1006 // Naming Styles
