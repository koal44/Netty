using System;
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

        public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _queue.Count;

        public void Clear() => _queue.Clear();

        public bool Any() => _queue.Any();

        public IEnumerable<T> GetAll() => _queue.Any() ? _queue.AsEnumerable() : Enumerable.Empty<T>();

        public T Peek() => _queue.Peek();

        /// <summary>
        /// Adds an item to the buffer. If the buffer is full, the oldest item will be dropped.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        public void Add(T item)
        {
            if (_queue.Count == Capacity)
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
