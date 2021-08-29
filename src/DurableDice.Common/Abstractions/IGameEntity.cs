using DurableDice.Common.Models.Commands;

namespace DurableDice.Common.Abstractions;

public interface IGameEntity
{
    Task<bool> AddPlayerAsync(AddPlayerCommand command);

    Task<bool> AttackFieldAsync(AttackMoveCommand command);

    Task<bool> EndRoundAsync(string playerId);

    Task<bool> RemovePlayerAsync(string playerId);

    Task<bool> StartMatchAsync();
}

