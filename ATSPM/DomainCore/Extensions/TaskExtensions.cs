using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ControllerLogger.Helpers
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Trigger a Task in an async method without having to await
        /// Otherwise you have to markd the tas with #pragma warning disable CS4014
        /// </summary>
        /// <param name="task"></param>
        public static void FireAndForget(this Task task)
        {
        }

        public static Task<T> AsTask<T>(this T o)
        {
            return Task.Run(() => o);
        }
    }
}
