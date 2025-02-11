using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Utah.Udot.Atspm.Data.Utility
{
    /// <summary>
    /// Value comparer for change tracking dictionaries
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal class DictionaryValueComparer<TKey, TValue> : ValueComparer<Dictionary<TKey, TValue>>
    {
        public DictionaryValueComparer() : base(
            (a, b) => Compare(a, b),
            o => GetHashCode(o),
            o => GetSnapshot(o))
        { }

        private static bool Compare(Dictionary<TKey, TValue> a, Dictionary<TKey, TValue> b)
        {
            var e1 = Enumerable.SequenceEqual(a.Keys.ToList(), b.Keys.ToList());
            var e2 = Enumerable.SequenceEqual(a.Values.ToList(), b.Values.ToList());

            return e1 && e2;
        }

        private static int GetHashCode(Dictionary<TKey, TValue> obj)
        {
            var hash = new HashCode();

            foreach (var h in obj)
            {
                hash.Add(h.GetHashCode());
            }

            return hash.ToHashCode();
        }

        private static Dictionary<TKey, TValue> GetSnapshot(Dictionary<TKey, TValue> obj)
        {
            return obj.ToDictionary();
        }
    }
}
