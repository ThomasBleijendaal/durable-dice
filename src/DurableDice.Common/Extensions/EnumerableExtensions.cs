namespace DurableDice.Common.Extensions;

public static class EnumerableExtensions
{
    public static T? MaxOrDefault<T>(this IEnumerable<T> source)
        => source.Any() ? source.Max() : default;
}
