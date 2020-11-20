using System.Threading.Tasks;

namespace imageStacker.Core.Abstraction
{
    public interface IBoundedQueue<T>
    {
        Task Enqueue(T item);
        /// <summary>
        /// Wait until an item is available, return null if queue is already completed.
        /// </summary>
        /// <returns></returns>
        Task<T> DequeueOrNull();
        /// <summary>
        /// Wait until an item is available, throw if queue is already completed.
        /// </summary>
        /// <returns></returns>
        Task<T> Dequeue();
        bool IsCompleted { get; }
        bool IsAddingCompleted { get; }
        int Count { get; }
        void CompleteAdding();
    }

    public class BoundedQueueFactory
    {
        public static IBoundedQueue<T> Get<T>(int capacity)
        {
            return new BlockingCollectionBoundedQueue<T>(capacity);
        }
    }
}
