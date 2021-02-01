using System.Threading.Tasks;

namespace imageStacker.Core.Abstraction
{
    public interface IBoundedQueue
    {
        /// <summary>
        /// Wait until an item is available, return null if queue is already completed.
        /// </summary>
        /// <returns></returns>
        bool IsCompleted { get; }
        bool IsAddingCompleted { get; }
        int Count { get; }
        string Name { get; }
        int AddedCount { get; }
    }
    public interface IBoundedQueue<T> : IBoundedQueue
    {
        Task Enqueue(T item);
        /// <summary>
        /// Wait until an item is available, return null if queue is already completed.
        /// </summary>
        /// <returns></returns>
        Task<T> DequeueOrDefault();
        void CompleteAdding();
    }

    public class BoundedQueueFactory
    {
        public static ILogger Logger { get; set; }
        public static IBoundedQueue<T> Get<T>(int capacity, string name)
        {
            var queue = new SemaphoreBoundedQueue<T>(capacity, name);
            Logger?.AddQueue(queue);
            return queue;
        }
    }
}
