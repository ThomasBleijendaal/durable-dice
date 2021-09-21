using DurableDice.Common.Models.State;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace DurableDice.Common.Models.History
{
    public class GameHistory : TableEntity
    {
        public GameHistory()
        {
        }

        public GameHistory(string gameId, GameState gameState)
        {
            PartitionKey = gameId;
            RowKey = gameId;

            SetFields(gameState.Fields.Select(x => new GameField { FieldId = x.Id }).ToList());
            SetPlayers(gameState.Players.Select(x => new GamePlayer { PlayerId = x.Id }).ToList());
            SetCount(new List<GameCount> { GetCountForGame(gameState) });
        }

        public int GameRound { get; set; }

        public string Fields { get; set; } = "";

        public List<GameField> GetFields() => JsonConvert.DeserializeObject<List<GameField>>(Fields) ?? new List<GameField>();

        private void SetFields(List<GameField> fields) => Fields = JsonConvert.SerializeObject(fields);

        public string Players { get; set; } = "";

        public List<GamePlayer> GetPlayers() => JsonConvert.DeserializeObject<List<GamePlayer>>(Players) ?? new List<GamePlayer>();

        private void SetPlayers(List<GamePlayer> players) => Players = JsonConvert.SerializeObject(players);

        public string Count { get; set; } = "";

        public List<GameCount> GetCount() => JsonConvert.DeserializeObject<List<GameCount>>(Count) ?? new List<GameCount>();

        private void SetCount(List<GameCount> diceCount) => Count = JsonConvert.SerializeObject(diceCount);

        public void AddState(GameState state)
        {
            var fields = GetFields();
            var players = GetPlayers();
            var count = GetCount();

            if (state.PreviousAttack is Attack attack)
            {
                var attacker = players.First(x => x.PlayerId == attack.AttackerId);
                attacker.NumberOfFieldsCaptured++;

                var defender = players.First(x => x.PlayerId == attack.DefenderId);
                defender.NumberOfFieldsLost++;

                var attackingField = fields.First(x => x.FieldId == attack.AttackingFieldId);
                attackingField.NumberOfActions++;

                if (attack.IsSuccessful)
                {
                    var capturedField = fields.First(x => x.FieldId == attack.DefendingFieldId);
                    capturedField.NumberOfActions++;
                }
            }

            if (state.GameRound > GameRound)
            {
                count.Add(GetCountForGame(state));
                GameRound = state.GameRound;
            }

            SetFields(fields);
            SetPlayers(players);
            SetCount(count);
        }

        private static GameCount GetCountForGame(GameState gameState)
            => new GameCount
            {
                FieldCount = GetFieldCountPerPlayer(gameState),
                DiceCount = GetDiceCountPerPlayer(gameState)
            };

        private static List<int> GetFieldCountPerPlayer(GameState gameState)
            => gameState.Players.Select(gameState.PlayerFieldCount).ToList();

        private static List<int> GetDiceCountPerPlayer(GameState gameState)
            => gameState.Players.Select(p => gameState.Fields.Where(f => f.OwnerId == p.Id).Sum(x => x.DiceCount)).ToList();
    }
}
