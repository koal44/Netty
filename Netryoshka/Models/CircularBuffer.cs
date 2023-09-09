using System;
using System.Collections.Generic;
using System.Linq;

namespace Netty.Models
{
    /// <summary>
    /// A FIFO queue where elements at the end of the queue are dropped when max size has been reached.
    /// </summary>
    /// <typeparam name="T">The type of items in the queue.</typeparam>
    public class CircularBuffer<T>
    {
        private readonly Queue<T> _queue;
        private readonly int _maxSize;

        public CircularBuffer(int maxSize)
        {
            _queue = new Queue<T>(maxSize);
            _maxSize = maxSize;
        }

        public int Count => _queue.Count;

        public void Clear() => _queue.Clear();

        public bool Any() { return _queue.Any(); }

        public IEnumerable<T> GetAll() { return _queue.Any() ? _queue.AsEnumerable() : Enumerable.Empty<T>(); }

        public T Peek()
        {
            if (!Any())
            {
                throw new InvalidOperationException("Cannot peek an empty CircularBuffer.");
            }

            return _queue.Peek();
        }

        /// <summary>
        /// Adds an item to the buffer. If the buffer is full, the oldest item will be dropped.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Add(T item)
        {
            if (_queue.Count == _maxSize)
            {
                _queue.Dequeue();  // Remove oldest item if we've reached max size
            }
            _queue.Enqueue(item);
        }

        /// <summary>
        /// Adds a range of items to the buffer. If the total count exceeds the max size, the oldest items will be dropped.
        /// </summary>
        /// <param name="items">The items to be added to the buffer.</param>
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }



    }
}
