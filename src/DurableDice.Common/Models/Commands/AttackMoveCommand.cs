namespace DurableDice.Common.Models.Commands;

public record AttackMoveCommand(string PlayerId, string FromFieldId, string ToFieldId);
