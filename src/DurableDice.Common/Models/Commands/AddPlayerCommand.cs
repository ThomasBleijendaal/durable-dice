namespace DurableDice.Common.Models.Commands;

public record AddPlayerCommand(string GameId, string PlayerId, string PlayerName);
