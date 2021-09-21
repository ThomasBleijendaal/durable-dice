namespace DurableDice.Common.Helpers
{
    public static class DiceHelper
    {
        private static readonly Random Random = new Random();

        public static IEnumerable<int> ThrowDice(int numberOfDice)
            => Enumerable.Range(0, numberOfDice)
                .Select(i => Random.Next(1, 7));
    }
}
