using System.Security.Cryptography;
using DurableDice.Common.Abstractions;
using DurableDice.Common.Extensions;
using DurableDice.Common.Geometry;
using DurableDice.Common.Helpers;
using DurableDice.Common.Models.Commands;
using DurableDice.Common.Models.State;
using DurableDice.Common.Services;
using DurableDice.Functions.SignalRFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace DurableDice.Functions.Entities;

public class GameEntity : GameState, IGameEntity
{
    private readonly IAsyncCollector<SignalRMessage> _signalr;
    private readonly string _gameId;
    private readonly GameHistoryService _gameHistoryService;

    public GameEntity(
        IAsyncCollector<SignalRMessage> signalr,
        string gameId,
        GameHistoryService gameHistoryService)
    {
        _signalr = signalr;
        _gameId = gameId;
        _gameHistoryService = gameHistoryService;
    }

    [FunctionName(nameof(GameEntity))]
    public static Task ProxyGameEntityFunction(
        [EntityTrigger] IDurableEntityContext context,
        [SignalR(HubName = nameof(GameHub))] IAsyncCollector<SignalRMessage> signalr)
            => context.DispatchAsync<GameEntity>(
                signalr,
                context.EntityKey,
                context.FunctionBindingContext.CreateObjectInstance<GameHistoryService>());

    public async Task AddBotAsync(AddBotCommand command)
    {
        if (Players.Count == 8)
        {
            return;
        }

        Players.Add(new Player
        {
            Id = Guid.NewGuid().ToString(),
            Name = BotHelper.BotName(command.BotType, Players.Count),
            BotType = command.BotType,
            IsReady = true
        });

        await DistributeStateAsync();
    }

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

    public async Task MoveFieldAsync(MoveCommand command)
    {
        var fromField = Fields.FirstOrDefault(x => x.Id == command.FromFieldId);
        var toField = Fields.FirstOrDefault(x => x.Id == command.ToFieldId);

        if (fromField == null ||
            toField == null ||
            fromField.OwnerId != command.PlayerId ||
            fromField.DiceCount <= 1 ||
            !Geometry.AreNeighboringFields(fromField.Id, toField.Id))
        {
            return;
        }

        PreviousMove = null;
        PreviousAttack = null;

        if (toField.OwnerId != ActivePlayerId)
        {
            await AttackAsync(fromField, toField);
        }
        else if (ActivePlayer.DiceMovesThisTurn < Rules.MaxDiceMovedPerTurn)
        {
            Move(fromField, toField);
        }

        await DistributeStateAsync();
    }

    public async Task EndRoundAsync(string playerId)
    {
        if (ActivePlayerId == playerId && await HandleEndOfRoundAsync())
        {
            await HandleBotsAsync();
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

    public async Task ReadyWithRulesAsync(ReadyPlayerCommand command)
    {
        if (Players.ElementAtOrDefault(0)?.Id == command.PlayerId)
        {
            Rules = command.GameRules;
        }

        await ReadyAsync(command.PlayerId);
    }

    public async Task ReadyAsync(string playerId)
    {
        Players.First(x => x.Id == playerId).IsReady = true;

        if (Players.Count > 1 && Players.All(x => x.IsReady))
        {
            Rules.EnsureValidRules();

            ActivePlayerId = Players.Where(x => !x.BotType.HasValue).RandomItem().Id;

            var fieldCountPerPlayer = (32 / Players.Count) * (Rules.StartDiceCountPerField - 1);

            Fields = FieldGenerator.GenerateFields(Players);

            foreach (var player in Players)
            {
                AddNewDiceToPlayer(player.Id, fieldCountPerPlayer);
            }

            CalculateContinuousFieldCountsForAllPlayers();

            Fields.ForEach(x => x.DiceAdded = 0);

            Players.ForEach(x => x.DiceBuffer = Rules.InitialDiceBuffer);
        }

        await DistributeStateAsync();
    }

    private async Task AttackAsync(Field fromField, Field toField)
    {
        Fields.ForEach(x => x.DiceAdded = 0);

        var attackThrow = DiceHelper.ThrowDice(fromField.DiceCount).ToList();
        var defendThrow = DiceHelper.ThrowDice(toField.DiceCount).ToList();

        PreviousAttack = new Attack
        {
            AttackerId = fromField.OwnerId,
            AttackingFieldId = fromField.Id,
            AttackingDiceCount = attackThrow,
            DefenderId = toField.OwnerId,
            DefendingFieldId = toField.Id,
            DefendingDiceCount = defendThrow,
            IsSuccessful = attackThrow.Sum() > defendThrow.Sum()
        };

        if (PreviousAttack.IsSuccessful)
        {
            toField.DiceCount = fromField.DiceCount - 1;
            toField.OwnerId = fromField.OwnerId;
        }

        fromField.DiceCount = 1;

        CalculateContinuousFieldCountsForAllPlayers();

        await _gameHistoryService.AddGameStateAsync(_gameId, this);
    }

    private void Move(Field fromField, Field toField)
    {
        var diceToMove = Math.Min(Rules.MaxDiceMovedPerTurn - ActivePlayer.DiceMovesThisTurn, 
            Math.Min(toField.MaxDiceAllowedToAdd, fromField.DiceCount - 1));

        if (diceToMove == 0)
        {
            return;
        }

        fromField.DiceCount -= diceToMove;
        toField.DiceCount += diceToMove;

        PreviousMove = new Move
        {
            Count = diceToMove,
            AddedFieldId = toField.Id
        };

        ActivePlayer.DiceMovesThisTurn += diceToMove;

        return;
    }

    private async Task<bool> HandleEndOfRoundAsync()
    {
        if (ActivePlayerId == null)
        {
            return false;
        }

        Fields.ForEach(x => x.DiceAdded = 0);

        var multiplier = Rules.DiceGenerationMultiplier + (Players.Count(x => x.ContinuousFieldCount == 0) * Rules.DeadPlayerMultiplier);

        var diceBuffer = (int)((Geometry.GetLargestContinuousFieldBlock(ActivePlayerId) * multiplier) + ActivePlayer.DiceBuffer);

        diceBuffer = AddNewDiceToPlayer(ActivePlayerId, diceBuffer);

        ActivePlayer.DiceBuffer = diceBuffer;
        ActivePlayer.DiceMovesThisTurn = 0;

        var gameEnded = false;

        if (!Fields.All(x => x.OwnerId == ActivePlayerId))
        {
            SelectNextActivePlayer(ActivePlayerId);
        }
        else
        {
            gameEnded = true;
        }

        PreviousAttack = null;
        PreviousMove = null;

        GameRound++;

        await _gameHistoryService.AddGameStateAsync(_gameId, this);
        await DistributeStateAsync();

        return !gameEnded;
    }

    private async Task HandleBotsAsync()
    {
        while (ActivePlayer.BotType.HasValue)
        {
            await HandleBotMovesAsync();
            if (!await HandleEndOfRoundAsync())
            {
                return;
            }
        }
    }

    private async Task HandleBotMovesAsync()
    {
        do
        {
            var allBots = Players.Where(x => x.ContinuousFieldCount > 0).All(x => x.BotType.HasValue);

            if (!allBots)
            {
                await Task.Delay(500);
            }

            var bot = BotHelper.BuildBot(this);

            if (bot.MakeMove() is not MoveCommand command)
            {
                return;
            }

            await MoveFieldAsync(command);
        }
        while (true);
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
        if (Winner != null || Players.Count == 0)
        {
            return;
        }

        var currentIndex = Players.FindIndex(x => x.Id == playerId) + 1;

        var activePlayer = Players
            .Select((player, index) => (player, index: (index - currentIndex + Players.Count) % Players.Count))
            .Where(x => PlayerFieldCount(x.player) > 0)
            .OrderBy(x => x.index)
            .FirstOrDefault()
            .player;

        ActivePlayerId = activePlayer.Id;
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
                var diceTaken = RandomNumberGenerator.GetInt32(Math.Min(field.MaxDiceAllowedToAdd, diceBuffer) + 1);

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

    private void CalculateContinuousFieldCountsForAllPlayers()
    {
        foreach (var player in Players)
        {
            player.ContinuousFieldCount = Geometry.GetLargestContinuousFieldBlock(player.Id);
        }
    }
}
