namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Checks to see if <paramref name="enumerable"/> is null or empty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }
}