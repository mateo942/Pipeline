using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Pipeline
{
    public class Variables : Dictionary<string, object>
    {
        public Variables()
        {
        }

        public void AddRange(IDictionary<string, object> v)
        {
            if (v == null)
                return;

            foreach (var item in v)
            {
                this[item.Key] = item.Value;
            }
        }

        public T Get<T>(string key)
        {
            if (this.TryGetValue(key, out object t))
            {
                T result;
                if(t is string tString && typeof(T) != typeof(string))
                {
                    result = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(tString);
                }
                else if(t is T)
                {
                    result = (T)t;
                } else
                {
                    result = (T)Convert.ChangeType(t, typeof(T));

                }

                return result;
            }

            throw new KeyNotFoundException($"Key: {key}");
        }

        public bool TryGet<T>(string key, out T @out)
        {
            try
            {
                @out = Get<T>(key);

                return true;
            }
            catch
            {
                @out = default(T);
                return false;
            }
        }

        public bool TryGetEnumerate<T>(string key, out IEnumerable<T> @out)
        {
            if(TryGet<IEnumerable<T>>(key, out @out))
            {
                return true;
            }

            if(TryGet<T>(key, out T @singleOut))
            {
                var tmp = new T[1];
                tmp[0] = singleOut;

                @out = tmp;
                return true;
            }

            return false;
        }

        public T Get<T>(string key, T defaultValue)
        {
            var r = Get<T>(key);
            if (r.Equals(default(T)))
            {
                Set<T>(key, defaultValue);
                return defaultValue;
            }

            return r;
        }

        public void Set<T>(string key, T value)
        {
            this[key] = value;
        }

        public List<T> AddElementToList<T>(string key, T value)
        {
            var list = Get<List<T>>(key);
            if(list == null)
            {
                list = new List<T>();
                Set(key, list);
            }

            list.Add(value);

            return list;
        }
    }
}
