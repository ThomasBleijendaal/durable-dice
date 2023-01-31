using DurableDice.Common.Models.State;

namespace DurableDice.Common.Models.Commands;

public record ReadyPlayerCommand(string PlayerId, GameRules GameRules);
