namespace DurableDice.Common.Extensions;

public static class EnumerableExtensions
{
    public static T? MaxOrDefault<T>(this IEnumerable<T> source)
        => source.Any() ? source.Max() : default;

    public static T RandomItem<T>(this IEnumerable<T> source)
        => source.OrderBy(x => Guid.NewGuid()).First();
}
