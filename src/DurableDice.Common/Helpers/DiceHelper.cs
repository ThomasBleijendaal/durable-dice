using System.Security.Cryptography;

namespace DurableDice.Common.Helpers
{
    public static class DiceHelper
    {
        public static IEnumerable<int> ThrowDice(int numberOfDice)
            => Enumerable.Range(0, numberOfDice)
                .Select(i => RandomNumberGenerator.GetInt32(6) + 1);
    }
}
