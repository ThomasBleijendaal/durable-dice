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
            fromField.DiceCount <= 1 ||
            !Geometry.AreNeighboringFields(fromField.Id, toField.Id))
        {
            await DistributeStateAsync();
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
            var diceBuffer = Geometry.GetLargestContinuousFieldBlock(playerId) + ActivePlayer.DiceBuffer;

            diceBuffer = AddNewDiceToPlayer(ActivePlayerId, diceBuffer);

            ActivePlayer.DiceBuffer = diceBuffer;

            SelectNextActivePlayer(playerId);

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

        ActivePlayerId = Players[_random.Next(0, Players.Count)].Id;

        var fieldCountPerPlayer = 32 / Players.Count;

        Fields = FieldGenerator.GenerateFields(Players);

        foreach (var player in Players)
        {
            AddNewDiceToPlayer(player.Id, fieldCountPerPlayer);
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
        var index = Players.FindIndex(x => x.Id == playerId);
        if (++index >= Players.Count)
        {
            index = 0;
        }

        ActivePlayerId = Players[index].Id;
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
