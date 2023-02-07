namespace DurableDice.Common.Extensions;

public static class EnumerableExtensions
{
    public static T? MaxOrDefault<T>(this IEnumerable<T> source)
        => source.Any() ? source.Max() : default;

    public static T RandomItem<T>(this IEnumerable<T> source)
        => source.Shuffle().First();

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random? random = null)
        => source.OrderBy(x => (random ?? Random.Shared).Next());
}
