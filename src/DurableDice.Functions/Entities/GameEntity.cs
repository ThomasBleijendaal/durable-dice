using DurableDice.Common.Abstractions;
using DurableDice.Common.Helpers;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;
using DurableDice.Functions.SignalRFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace DurableDice.Functions.Entities;

public class GameEntity : GameState, IGameEntity
{
    private readonly IAsyncCollector<SignalRMessage> _signalr;
    private readonly string _gameId;

    public GameEntity(
        IAsyncCollector<SignalRMessage> signalr,
        string gameId)
    {
        _signalr = signalr;
        _gameId = gameId;
    }

    public async Task AddPlayerAsync(AddPlayerCommand command)
    {
        if (string.IsNullOrEmpty(OwnerId) || !Players.Any(x => x.Id == OwnerId))
        {
            OwnerId = command.PlayerId;
        }

        Players.Add(new Player { Id = command.PlayerId, Name = command.PlayerName });

        await DistributeStateAsync();
    }

    public async Task AttackFieldAsync(AttackMoveCommand command)
    {
        var fromField = Fields.FirstOrDefault(x => x.Id == command.FromFieldId);
        var toField = Fields.FirstOrDefault(x => x.Id == command.ToFieldId);

        if (fromField == null ||
            toField == null ||
            fromField.OwnerId != command.PlayerId ||
            command.PlayerId != ActivePlayerId ||
            fromField.DiceCount <= 1)
        {
            return;
        }

        // TODO: expose this attack in state change

        var attackThrow = DiceHelper.ThrowDice(fromField.DiceCount);
        var defendThrow = DiceHelper.ThrowDice(toField.DiceCount);

        if (attackThrow > defendThrow)
        {
            toField.DiceCount = fromField.DiceCount - 1;
            toField.OwnerId = fromField.OwnerId;
        }

        fromField.DiceCount = 1;
        
        await DistributeStateAsync();
    }

    public async Task EndRoundAsync(string playerId)
    {
        if (ActivePlayerId == playerId)
        {
            var index = Players.FindIndex(x => x.Id == playerId);
            if (++index >= Players.Count)
            {
                index = 0;
            }

            ActivePlayerId = Players[index].Id;

            await DistributeStateAsync();
        }
    }

    public async Task RemovePlayerAsync(string playerId)
    {
        Players.RemoveAll(x => x.Id == playerId);

        if (Players.Count == 0)
        {
            // destroy session
        }

        await DistributeStateAsync();
    }

    public async Task StartMatchAsync()
    {
        if (Players.Count < 2)
        {
            return;
        }

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

        await DistributeStateAsync();
    }

    private async Task DistributeStateAsync()
        => await _signalr.AddAsync(new SignalRMessage
        {
            GroupName = _gameId,
            Arguments = new object[] { this },
            Target = "Broadcast"
        });

    [FunctionName(nameof(GameEntity))]
    public static Task ProxyGameEntityFunction(
        [EntityTrigger] IDurableEntityContext context,
        [SignalR(HubName = nameof(GameHub))] IAsyncCollector<SignalRMessage> signalr)
            => context.DispatchAsync<GameEntity>(signalr, context.EntityKey);
}
