using DurableDice.Common.Models.Commands;

namespace DurableDice.Common.Abstractions;

public interface IGameEntity
{
    Task AddBotAsync(AddBotCommand command);

    Task AddPlayerAsync(AddPlayerCommand command);

    Task MoveFieldAsync(MoveCommand command);

    Task EndRoundAsync(string playerId);

    Task RemovePlayerAsync(string playerId);

    Task ReadyWithRulesAsync(ReadyPlayerCommand command);

    Task ReadyAsync(string playerId);
}
