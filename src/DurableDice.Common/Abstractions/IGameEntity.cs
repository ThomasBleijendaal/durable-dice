using DurableDice.Common.Models.Commands;

namespace DurableDice.Common.Abstractions;

public interface IGameEntity
{
    Task AddPlayerAsync(AddPlayerCommand command);

    Task AttackFieldAsync(AttackMoveCommand command);

    Task EndRoundAsync(string playerId);

    Task RemovePlayerAsync(string playerId);

    Task ReadyAsync(string playerId);
}

