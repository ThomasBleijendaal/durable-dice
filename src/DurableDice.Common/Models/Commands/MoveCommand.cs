namespace DurableDice.Common.Models.Commands;

public record MoveCommand(string PlayerId, string FromFieldId, string ToFieldId);
