using System;
using System.Collections.Immutable;

namespace ConsoleApplication1
{
    public class Scene
    {
        public Guid ID { get; } = Guid.NewGuid();

        public readonly Scene Parent;

        private IImmutableDictionary<Guid, object> _data;

        public Scene() : this(ImmutableDictionary<Guid, object>.Empty)
        {
        }

        private Scene(IImmutableDictionary<Guid, object> data)
        {
            _data = data;
        }

        public Scene(Scene parent) : this(parent._data)
        {
            Parent = parent;
        }

        public object this[Guid id]
        {
            get
            {
                object value;
                return _data.TryGetValue(id, out value) ? value : null;
            }
            set
            {
                _data = _data.SetItem(id, value);
            }
        }

        public void Set<T>(Guid id, T value)
        {
            this[id] = value;
        }

        public T Get<T>(Guid id)
        {
            var value = this[id];
            return value is T ? (T)value : default(T);
        }

        public bool Exists(Guid id)
        {
            return _data.TryGetKey(id, out id);
        }
    }
}