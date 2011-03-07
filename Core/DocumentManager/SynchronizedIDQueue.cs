using System.Collections;
using System.Collections.Generic;

namespace GKManagers
{
    /// <summary>
    /// Provides thread-safe access to a Queue containing ID values.
    /// </summary>
    class SynchronizedIDQueue
    {
        /// <summary>
        /// The Queue.
        /// </summary>
        private Queue<int> _theQueue;

        /// <summary>
        /// Constructor
        /// </summary>
        public SynchronizedIDQueue()
        {
            _theQueue = new Queue<int>();
        }

        #region Properties

        /// <summary>
        /// Gets the number of items remaining in the queue.
        /// </summary>
        public int Count
        {
            get
            {
                ICollection ic = _theQueue;

                lock (ic.SyncRoot)
                {
                    return _theQueue.Count;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Removes all items from the queue.
        /// </summary>
        public void Clear()
        {
            ICollection ic = _theQueue;

            lock (ic.SyncRoot)
            {
                _theQueue.Clear();
            }
        }

        /// <summary>
        /// Test for whether the queue contains an entry for item.
        /// </summary>
        /// <param name="item">The value to locate in the queue.</param>
        /// <returns>True if item is located; false otherwise.</returns>
        public bool Contains(int item)
        {
            ICollection ic = _theQueue;

            lock (ic.SyncRoot)
            {
                return _theQueue.Contains(item);
            }
        }

        /// <summary>
        /// Copies the elements of the queue to an already allocated array,
        /// starting at the specified array index.
        /// </summary>
        /// <param name="array">Array to copy the queue values into</param>
        /// <param name="arrayIndex">The offset within the array to begin copying to.</param>
        public void CopyTo(int[] array, int arrayIndex)
        {
            ICollection ic = _theQueue;

            lock (ic.SyncRoot)
            {
                _theQueue.CopyTo(array, arrayIndex);
            }
        }

        /// <summary>
        /// Removes and returns the value at the beginning of the queue.
        /// Throws InvalidOperationException if the queue is empty.
        /// </summary>
        /// <returns>The next value in the queue.</returns>
        public int Dequeue()
        {
            ICollection ic = _theQueue;

            lock (ic.SyncRoot)
            {
                return _theQueue.Dequeue();
            }
        }

        /// <summary>
        /// Adds a value at the end of the queue.
        /// </summary>
        /// <param name="item">The value to be added.</param>
        public void Enqueue(int item)
        {
            ICollection ic = _theQueue;

            lock (ic.SyncRoot)
            {
                _theQueue.Enqueue(item);
            }
        }

        /// <summary>
        /// Returns the value at the beginning of the queue without removing it.
        /// </summary>
        /// <returns>The next value in the queue</returns>
        public int Peek()
        {
            ICollection ic = _theQueue;

            lock (ic.SyncRoot)
            {
                return _theQueue.Peek();
            }
        }

        /// <summary>
        /// Copies all values in the queue to an array.
        /// </summary>
        /// <returns>An array containing the contents of the queue.</returns>
        public int[] ToArray()
        {
            ICollection ic = _theQueue;

            lock (ic.SyncRoot)
            {
                return _theQueue.ToArray();
            }
        }

        /// <summary>
        /// Sets the capacity of the quue to the actual number of elements
        /// it contains.
        /// </summary>
        public void TrimExcess()
        {
            ICollection ic = _theQueue;

            lock (ic.SyncRoot)
            {
                _theQueue.TrimExcess();
            }
        }

        #endregion
    }
}
