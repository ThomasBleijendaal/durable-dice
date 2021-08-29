namespace DurableDice.Common.Models.Commands;

public record AttackMoveCommand(string GameId, string PlayerId, string FromFieldId, string ToFieldId);
