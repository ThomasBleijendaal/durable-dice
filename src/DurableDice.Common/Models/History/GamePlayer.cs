namespace DurableDice.Common.Models.History
{
    public class GamePlayer
    {
        public string PlayerId { get; set; } = null!;
        public int NumberOfFieldsCaptured { get; set; }
        public int NumberOfFieldsLost { get; set; }
    }
}
