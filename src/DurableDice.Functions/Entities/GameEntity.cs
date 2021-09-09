using DurableDice.Common.Abstractions;
using DurableDice.Common.Geometry;
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
    private readonly Random _random = new Random();

    public GameEntity(
        IAsyncCollector<SignalRMessage> signalr,
        string gameId)
    {
        _signalr = signalr;
        _gameId = gameId;
    }

    [FunctionName(nameof(GameEntity))]
    public static Task ProxyGameEntityFunction(
        [EntityTrigger] IDurableEntityContext context,
        [SignalR(HubName = nameof(GameHub))] IAsyncCollector<SignalRMessage> signalr)
            => context.DispatchAsync<GameEntity>(signalr, context.EntityKey);

    public async Task AddPlayerAsync(AddPlayerCommand command)
    {
        if (Players.Count == 8)
        {
            return;
        }

        Players.Add(new Player
        {
            Id = command.PlayerId,
            Name = command.PlayerName?.Substring(0, Math.Min(command.PlayerName.Length, 16)) ?? "Dummy"
        });

        await DistributeStateAsync();
    }

    public async Task AttackFieldAsync(AttackMoveCommand command)
    {
        Fields.ForEach(x => x.DiceAdded = 0);

        var fromField = Fields.FirstOrDefault(x => x.Id == command.FromFieldId);
        var toField = Fields.FirstOrDefault(x => x.Id == command.ToFieldId);

        if (fromField == null ||
            toField == null ||
            fromField.OwnerId != command.PlayerId ||
            command.PlayerId != ActivePlayerId ||
            fromField.DiceCount <= 1 ||
            !Geometry.AreNeighboringFields(fromField.Id, toField.Id))
        {
            return;
        }

        var attackThrow = DiceHelper.ThrowDice(fromField.DiceCount);
        var defendThrow = DiceHelper.ThrowDice(toField.DiceCount);

        PreviousAttack = new Attack
        {
            AttackerId = fromField.OwnerId,
            AttackingFieldId = command.FromFieldId,
            AttackingDiceCount = attackThrow,
            DefenderId = toField.OwnerId,
            DefendingFieldId = command.ToFieldId,
            DefendingDiceCount = defendThrow,
            IsSuccessful = attackThrow > defendThrow
        };

        if (PreviousAttack.IsSuccessful)
        {
            toField.DiceCount = fromField.DiceCount - 1;
            toField.OwnerId = fromField.OwnerId;
        }

        fromField.DiceCount = 1;

        foreach (var player in Players)
        {
            player.ContinuousFieldCount = Geometry.GetLargestContinuousFieldBlock(player.Id);
        }

        await DistributeStateAsync();
    }

    public async Task EndRoundAsync(string playerId)
    {
        Fields.ForEach(x => x.DiceAdded = 0);

        if (ActivePlayerId == playerId)
        {
            var diceBuffer = Geometry.GetLargestContinuousFieldBlock(playerId) + ActivePlayer.DiceBuffer;

            diceBuffer = AddNewDiceToPlayer(ActivePlayerId, diceBuffer);

            ActivePlayer.DiceBuffer = diceBuffer;

            if (!Fields.All(x => x.OwnerId == playerId))
            {
                SelectNextActivePlayer(playerId);
            }

            PreviousAttack = null;

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

    public async Task ReadyAsync(string playerId)
    {
        Players.First(x => x.Id == playerId).Ready = true;

        if (Players.Count > 1 && Players.All(x => x.Ready))
        {
            ActivePlayerId = Players[_random.Next(0, Players.Count)].Id;

            var fieldCountPerPlayer = 32 / Players.Count;

            Fields = FieldGenerator.GenerateFields(Players);

            foreach (var player in Players)
            {
                AddNewDiceToPlayer(player.Id, fieldCountPerPlayer);
            }
        }

        await DistributeStateAsync();
    }

    private async Task DistributeStateAsync()
        => await _signalr.AddAsync(new SignalRMessage
        {
            GroupName = _gameId,
            Arguments = new object[] { this },
            Target = "Broadcast"
        });

    private void SelectNextActivePlayer(string playerId)
    {
        if (Winner != null)
        {
            return;
        }

        var newIndex = Players.FindIndex(x => x.Id == playerId) + 1;

        ActivePlayerId = newIndex == Players.Count
            ? Players[0].Id
            : Players
                .Select((player, index) => (player, index))
                .Where(x => PlayerFieldCount(x.player) > 0)
                .First(x => x.index >= newIndex)
                .player.Id;
    }

    private int AddNewDiceToPlayer(string playerId, int diceBuffer)
    {
        do
        {
            diceBuffer = AddNewDiceToFields(diceBuffer, PlayerRandomFields(playerId));
            if (diceBuffer == 0)
            {
                break;
            }
        }
        while (!ActivePlayerFields.All(x => x.MaxDiceAllowedToAdd == 0));

        return diceBuffer;

        int AddNewDiceToFields(int diceBuffer, IEnumerable<Field> playerFields)
        {
            foreach (var field in playerFields)
            {
                var diceTaken = _random.Next(0, Math.Min(field.MaxDiceAllowedToAdd, diceBuffer) + 1);

                field.DiceAdded = diceTaken;
                field.DiceCount += diceTaken;

                diceBuffer -= diceTaken;

                if (diceBuffer <= 0)
                {
                    break;
                }
            }

            return diceBuffer;
        }
    }
}
