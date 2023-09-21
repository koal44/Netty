using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Netryoshka.Models
{
    /// <summary>
    /// A FIFO queue where elements at the end of the queue are dropped when max size has been reached.
    /// </summary>
    /// <typeparam name="T">The type of items in the queue.</typeparam>
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly Queue<T> _queue;
        private readonly object _lock = new();

        public int Capacity { get; }

        public CircularBuffer(int capacity)
        {
            Capacity = capacity;
            _queue = new Queue<T>(Capacity);
        }

        public CircularBuffer(IEnumerable<T> items, int capacity)
        {
            Capacity = capacity;
            _queue = new Queue<T>(Capacity);
            AddRange(items);
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
            {
                return _queue.ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        /// <summary>
        /// Adds an item to the buffer. If the buffer is full, the oldest item will be dropped.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Add(T item)
        {
            lock (_lock)
            {
                if (_queue.Count == Capacity)
                {
                    _queue.Dequeue();
                }
                _queue.Enqueue(item);
            }
        }

        /// <summary>
        /// Adds a range of items to the buffer. If the total count exceeds the max size, the oldest items will be dropped.
        /// </summary>
        /// <param name="items">The items to be added to the buffer.</param>
        public void AddRange(IEnumerable<T> items)
        {
            lock (_lock)
            {
                foreach (var item in items)
                {
                    if (_queue.Count == Capacity)
                    {
                        _queue.Dequeue();
                    }
                    _queue.Enqueue(item);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
            }
        }

        public bool Any()
        {
            lock (_lock)
            {
                return _queue.Any();
            }
        }

        public IEnumerable<T> GetAll()
        {
            lock (_lock)
            {
                return _queue.ToList();
            }
        }

        public T Peek()
        {
            lock (_lock)
            {
                return _queue.Peek();
            }
        }
    }

}
