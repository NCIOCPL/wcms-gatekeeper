using System.Collections;
using System.Collections.Generic;

namespace GKManagers
{
    class SynchronizedIDQueue
    {
        private Queue<int> _theQueue;

        public SynchronizedIDQueue()
        {
            _theQueue = new Queue<int>();
        }

        #region Properties

        public int Count
        {
            get
            {
                ICollection ic = _theQueue;

                lock (ic)
                {
                    return _theQueue.Count;
                }
            }
        }

        #endregion

        #region Methods

        public void Clear()
        {
            ICollection ic = _theQueue;

            lock (ic)
            {
                _theQueue.Clear();
            }
        }

        public bool Contains(int item)
        {
            ICollection ic = _theQueue;

            lock (ic)
            {
                return _theQueue.Contains(item);
            }
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            ICollection ic = _theQueue;

            lock (ic)
            {
                _theQueue.CopyTo(array, arrayIndex);
            }
        }

        public int Dequeue()
        {
            ICollection ic = _theQueue;

            lock (ic)
            {
                return _theQueue.Dequeue();
            }
        }

        public void Enqueue(int item)
        {
            ICollection ic = _theQueue;

            lock (ic)
            {
                _theQueue.Enqueue(item);
            }
        }

        public int Peek()
        {
            ICollection ic = _theQueue;

            lock (ic)
            {
                return _theQueue.Peek();
            }
        }

        public int[] ToArray()
        {
            ICollection ic = _theQueue;

            lock (ic)
            {
                return _theQueue.ToArray();
            }
        }

        public void TrimExcess()
        {
            ICollection ic = _theQueue;

            lock (ic)
            {
                _theQueue.TrimExcess();
            }
        }

        #endregion
    }
}
