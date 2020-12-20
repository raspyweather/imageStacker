using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace imageStacker.Core.Extensions
{
    public static class ConcurrentQueueExtension
    {
        public static async Task WaitForBufferSpace<T>(this ConcurrentQueue<T> queue, int queueCapacity)
        {
            // IDEA: set delay dynamic depending on relative fill state 
            // e.g. for a queue size of 30 wait till at least 20% is free, therefore larger delay than a queue with a capacity of 8
            while (queue.Count > queueCapacity)
            {
                await Task.Delay(50);
                await Task.Yield();
            }
        }

        public static async Task<(bool cancelled, T item)> TryDequeueOrWait<T>(this ConcurrentQueue<T> queue, CancellationTokenSource token)
        {
            T nextImage;
            while (!queue.TryDequeue(out nextImage))
            {
                if (token.IsCancellationRequested)
                {
                    return (true, default);
                }

                await Task.Delay(25);
                await Task.Yield();
                continue;
            }

            return (false, nextImage);
        }
    }
}
