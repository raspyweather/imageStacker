using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace imageStacker.Core.Abstraction
{
    public class BlockingCollectionBoundedQueue<T> : IBoundedQueue<T>
    {
        private readonly BlockingCollection<T> _collection;

        public BlockingCollectionBoundedQueue(int capacity)
        {
            _collection = new BlockingCollection<T>(capacity);
        }

        public bool IsCompleted => _collection.IsCompleted;

        public bool IsAddingCompleted => _collection.IsAddingCompleted;

        public int Count => _collection.Count;

        public void CompleteAdding()
        {
            _collection.CompleteAdding();
        }

        public async Task<T> DequeueOrNull()
        {
            T item;
            while (!_collection.TryTake(out item))
            {
                if (_collection.IsCompleted)
                    return default;
                await Task.Delay(100);
            }
            return item;
        }

        public async Task<T> Dequeue()
        {
            T item;
            while(!_collection.TryTake(out item))
            {
                if (_collection.IsCompleted)
                    throw new InvalidOperationException("can't dequeue for a completed queue!");
                await Task.Delay(100);
            }
            return item;
        }

        public async Task Enqueue(T item)
        {
            while (!_collection.TryAdd(item))
            {
                await Task.Delay(100);
            }
        }
    }
}