using DurableDice.Common.Models.Commands;
using DurableDice.Functions.OrchestrationFunctions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
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
        string gameId)
    {
        await Groups.AddToGroupAsync(invocationContext.ConnectionId, gameId);
    }

    [FunctionName(nameof(AddPlayer))]
    public async Task AddPlayer(
        [SignalRTrigger] InvocationContext invocationContext,
        string gameId,
        string playerId,
        string playerName,
        [DurableClient] IDurableClient durableClient)
    {
        await durableClient.StartNewAsync(nameof(GameOrchestrationFunction.AddPlayerAsync), new AddPlayerCommand(gameId, playerId, playerName));
    }

    [FunctionName(nameof(StartMatch))]
    public async Task StartMatch(
        [SignalRTrigger] InvocationContext invocationContext,
        string gameId,
        [DurableClient] IDurableClient durableClient)
    {
        await durableClient.StartNewAsync(nameof(GameOrchestrationFunction.StartMatchAsync), new StartMatchCommand(gameId));
    }

    //[FunctionName(nameof(Broadcast))]
    //public async Task Broadcast(
    //    [SignalRTrigger] InvocationContext invocationContext, 
    //    string username, 
    //    string message)
    //{
    //    await Clients.All.SendAsync(nameof(Broadcast), username, message);
    //}

    [FunctionName(nameof(OnDisconnected))]
    public void OnDisconnected([SignalRTrigger] InvocationContext invocationContext)
    {

    }
}

#pragma warning restore IDE1006 // Naming Styles
