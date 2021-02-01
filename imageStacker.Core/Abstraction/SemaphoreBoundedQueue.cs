using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace imageStacker.Core.Abstraction
{
    public class SemaphoreBoundedQueue<T> : IBoundedQueue<T>
    {
        private readonly SemaphoreSlim _semaphoreAvailable;
        private readonly SemaphoreSlim _semaphoreOccupied;
        private readonly CancellationTokenSource _cts;
        private readonly ConcurrentQueue<T> _collection;
        public int AddedCount { get; private set; } = 0;
        public string Name { get; init; }

        public SemaphoreBoundedQueue(int capacity, string name)
        {
            _collection = new ConcurrentQueue<T>();
            _semaphoreAvailable = new SemaphoreSlim(capacity);
            _semaphoreOccupied = new SemaphoreSlim(0);
            _cts = new CancellationTokenSource();
            this.Name = name;
        }

        public bool IsCompleted { get; private set; }

        public bool IsAddingCompleted { get; private set; }

        public int Count => _collection.Count;

        public void CompleteAdding()
        {
            IsAddingCompleted = true;
            _semaphoreOccupied.Release();
        }

        public async Task<T> DequeueOrDefault()
        {
            try
            {
                await _semaphoreOccupied.WaitAsync(_cts.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                return default;
            }

            if (IsCompleted || !_collection.TryDequeue(out T item))
            {
                if (IsAddingCompleted)
                {
                    IsCompleted = true;
                    _cts.Cancel();
                    return default;
                }

                throw new Exception("something went wrong");
            }

            _semaphoreAvailable.Release();
            return item;
        }

        public async Task Enqueue(T item)
        {
            await _semaphoreAvailable.WaitAsync();
            AddedCount++;
            _collection.Enqueue(item);
            _semaphoreOccupied.Release();
        }
    }
}