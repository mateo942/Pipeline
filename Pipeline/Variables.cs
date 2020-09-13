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

        public void AddRange(IDictionary<string, string> v)
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
                var result = (T)Convert.ChangeType(t, typeof(T));

                return result;
            }

            return default;
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
    }
}
