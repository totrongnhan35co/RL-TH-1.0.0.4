using System.Collections.Generic;

namespace BarcodeVerificationSystem.Model
{
    public class SynchronizedDictonary<T, Y>
    {
        readonly object obj = new object();
        readonly Dictionary<T, Y> data = new Dictionary<T, Y>();

        public Y GetValue(T t)
        {
            lock (obj)
            {
                if(data.TryGetValue(t, out Y y))
                {
                    return y;
                }
                else
                {
                    return default;
                }
            }
        }

        public void SetValue(T t, Y y)
        {
            lock (obj)
            {
                data[t] = y;
            }
        }

        public void AddValue(T t, Y y)
        {
            lock (obj)
            {
                data.Add(t, y);
            }
        }
    }
}
