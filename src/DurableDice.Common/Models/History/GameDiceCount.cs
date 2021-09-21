using Newtonsoft.Json;

namespace DurableDice.Common.Models.History
{
    public class GameCount
    {
        [JsonProperty("d")]
        public List<int> DiceCount { get; set; } = null!;

        [JsonProperty("f")]
        public List<int> FieldCount { get; set; } = null!;
    }
}
