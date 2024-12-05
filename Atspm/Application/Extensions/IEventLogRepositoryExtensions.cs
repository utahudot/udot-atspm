using System.Collections;
using Utah.Udot.Atspm.Repositories.EventLogRepositories;

namespace Utah.Udot.Atspm.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IEventLogRepository"/>
    /// </summary>
    public static class IEventLogRepositoryExtensions
    {
        /// <summary>
        /// Used to update/insert an entry to the <see cref="IEventLogRepository"/> repository
        /// using a <see cref="HashSet{T}"/> to ensure there are no duplicate data events.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repo"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<T> Upsert<T>(this IEventLogRepository repo, T input) where T : CompressedEventLogBase
        {
            var searchLog = await repo.LookupAsync(input);

            if (searchLog != null)
            {
                dynamic list = Activator.CreateInstance(typeof(List<>).MakeGenericType(input.DataType));

                foreach (var i in Enumerable.Union(searchLog.Data, input.Data).ToHashSet())
                {
                    if (list is IList l)
                    {
                        l.Add(i);
                    }
                }

                searchLog.Data = list;

                await repo.UpdateAsync(searchLog);

                return (T)searchLog;
            }
            else
            {
                await repo.AddAsync(input);

                return input;
            }
        }
    }
}
