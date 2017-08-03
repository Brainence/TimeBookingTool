using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Formatters.Binary;

namespace TBT.App
{
    public class AppSettings
        : IDictionary<string, object>,
        ICollection<KeyValuePair<string, object>>,
        IEnumerable<KeyValuePair<string, object>>,
        IDictionary,
        ICollection,
        IEnumerable,
        IDisposable
    {
        private BinaryFormatter _formatter;
        private Dictionary<string, object> _settings;
        private IsolatedStorageFile _storageFile;
        public AppSettings()
        {
            _formatter = new BinaryFormatter();
            _settings = new Dictionary<string, object>();
            _storageFile = IsolatedStorageFile.GetUserStoreForAssembly();
            Load();
        }

        public ICollection<string> Keys
        {
            get { return ((IDictionary<string, object>)_settings).Keys; }
        }

        public ICollection<object> Values
        {
            get { return ((IDictionary<string, object>)_settings).Values; }
        }

        public int Count
        {
            get { return ((IDictionary<string, object>)_settings).Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IDictionary<string, object>)_settings).IsReadOnly; }
        }

        ICollection IDictionary.Keys
        {
            get { return ((IDictionary)_settings).Keys; }
        }

        ICollection IDictionary.Values
        {
            get { return ((IDictionary)_settings).Values; }
        }

        public bool IsFixedSize
        {
            get { return ((IDictionary)_settings).IsFixedSize; }
        }

        public object SyncRoot
        {
            get { return ((IDictionary)_settings).SyncRoot; }
        }

        public bool IsSynchronized
        {
            get { return ((IDictionary)_settings).IsSynchronized; }
        }

        public object this[object key]
        {
            get { return ((IDictionary)_settings)[key]; }
            set { ((IDictionary)_settings)[key] = value; }
        }

        public void Save()
        {
            using (var stream = _storageFile.OpenFile("settings.cfg", FileMode.OpenOrCreate, FileAccess.Write))
            {
                _formatter.Serialize(stream, _settings);
            }
        }

        public void Load()
        {
            if (_storageFile.FileExists("settings.cfg"))
            {
                using (var stream = _storageFile.OpenFile("settings.cfg", FileMode.OpenOrCreate, FileAccess.Read))
                {
                    _settings = (Dictionary<string, object>)_formatter.Deserialize(stream);
                }
            }
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, object>)_settings).ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            ((IDictionary<string, object>)_settings).Add(key, value);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, object>)_settings).Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return ((IDictionary<string, object>)_settings).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            ((IDictionary<string, object>)_settings).Add(item);
        }

        public void Clear()
        {
            ((IDictionary<string, object>)_settings).Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)_settings).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary<string, object>)_settings).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)_settings).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ((IDictionary<string, object>)_settings).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, object>)_settings).GetEnumerator();
        }

        public bool Contains(object key)
        {
            return ((IDictionary)_settings).Contains(key);
        }

        public void Add(object key, object value)
        {
            ((IDictionary)_settings).Add(key, value);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)_settings).GetEnumerator();
        }

        public void Remove(object key)
        {
            ((IDictionary)_settings).Remove(key);
        }

        public void CopyTo(Array array, int index)
        {
            ((IDictionary)_settings).CopyTo(array, index);
        }

        public object this[string key]
        {
            get { return _settings[key]; }
            set { _settings[key] = value; }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _storageFile.Dispose();
                }

                _formatter = null;
                _settings.Clear();

                disposedValue = true;
            }
        }

        ~AppSettings()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
