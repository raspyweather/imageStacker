using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
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
    }
}
