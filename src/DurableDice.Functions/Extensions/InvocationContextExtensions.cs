using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace DurableDice.Functions.Extensions;

public static class InvocationContextExtensions
{
    public static string GetGameId(this InvocationContext invocationContext)
        => invocationContext.Claims.TryGetValue("gid", out var gameId)
            ? gameId
            : throw new UnauthorizedAccessException();

    public static string GetPlayerId(this InvocationContext invocationContext)
        => invocationContext.Claims.TryGetValue("nameid", out var playerId)
            ? playerId
            : throw new UnauthorizedAccessException();
}
