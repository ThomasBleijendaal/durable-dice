using DurableDice.Common.Abstractions;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;

namespace DurableDice.Data.Entities;

public class GameEntity : GameState, IGameEntity
{
    public Task<bool> AddPlayerAsync(AddPlayerCommand command)
    {
        if (string.IsNullOrEmpty(OwnerId) || !Players.Any(x => x.Id == OwnerId))
        {
            OwnerId = command.PlayerId;
        }

        Players.Add(new Player { Id = command.PlayerId, Name = command.PlayerName });

        return Task.FromResult(true);
    }

    public Task<bool> AttackFieldAsync(AttackMoveCommand command)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EndRoundAsync(string playerId)
    {
        if (ActivePlayerId == playerId)
        {
            var index = Players.FindIndex(x => x.Id == playerId);
            index++;

            if (index > Players.Count)
            {
                index = 0;
            }

            ActivePlayerId = Players[index].Id;
        }

        return Task.FromResult(true);
    }

    public Task<bool> RemovePlayerAsync(string playerId)
    {
        Players.RemoveAll(x => x.Id == playerId);

        if (Players.Count == 0)
        {
            // destroy session
        }

        return Task.FromResult(true);
    }

    public Task<bool> StartMatchAsync()
    {
        var random = new Random();

        ActivePlayerId = Players[random.Next(0, Players.Count)].Id;

        Fields = Players.SelectMany(player =>
            Enumerable.Range(0, 5)
                .Select(i => new Field
                {
                    DiceCount = random.Next(1, 5),
                    Id = Guid.NewGuid().ToString(),
                    OwnerId = player.Id
                }))
            .OrderBy(x => Guid.NewGuid())
            .ToList();

        return Task.FromResult(true);
    }
}
