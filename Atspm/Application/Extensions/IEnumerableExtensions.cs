#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Extensions/IEnumerableExtensions.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Checks to see if <paramref name="source"/> is null or empty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        /// <summary>
        /// Splits the elements of a sequence into batches of a specified size.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to split into batches.</param>
        /// <param name="batchSize">The maximum number of elements in each batch.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of batches, where each batch is an <see cref="IEnumerable{T}"/> containing up to <paramref name="batchSize"/> elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            if (batchSize <= 0) ArgumentOutOfRangeException.ThrowIfNegativeOrZero(batchSize, nameof(batchSize));

            return source
                .Select((item, index) => new { item, index })
                .GroupBy(x => x.index / batchSize)
                .Select(group => group.Select(x => x.item));
        }

        /// <summary>
        /// Returns a sequence of lists representing a sliding window of the specified size over the source sequence.
        /// Each list contains consecutive elements from the source, and the window advances by one element at a time.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to create sliding windows from.</param>
        /// <param name="windowSize">The number of elements in each window.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of lists, where each list contains <paramref name="windowSize"/> consecutive elements from the source.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="windowSize"/> is less than 1.</exception>
        public static IEnumerable<IList<T>> SlidingWindow<T>(this IEnumerable<T> source, int windowSize)
        {
            if (windowSize < 1)
                throw new ArgumentException("Window size must be at least 1.");

            var buffer = new Queue<T>();

            foreach (var item in source)
            {
                buffer.Enqueue(item);

                if (buffer.Count == windowSize)
                {
                    yield return buffer.ToList();
                    buffer.Dequeue();
                }
            }
        }

    }
}
