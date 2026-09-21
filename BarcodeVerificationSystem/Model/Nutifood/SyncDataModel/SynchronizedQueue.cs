using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BarcodeVerificationSystem.Model
{
    public class SynchronizedQueue<T>
    {
        readonly Queue<T> _Queue = new Queue<T>();
        readonly object _Obj = new object();
        public void Enqueue(T item)
        {
            lock (_Obj)
            {
                _Queue.Enqueue(item);
                Monitor.PulseAll(_Obj);
            }
        }
        public T Dequeue()
        {
            lock (_Obj)
            {
                while (_Queue.Count() == 0)
                    Monitor.Wait(_Obj);
                return _Queue.Dequeue();
            }
        }
        public int Count()
        {
            lock (_Obj)
                return _Queue.Count();
        }
        public void Clear()
        {
            lock (_Obj)
                _Queue.Clear();
        }
    }
}
