namespace UwU.Events
{
    using System;
    using System.Collections.Generic;

    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _listeners = new();

        public void Subscribe<T>(Action<T> listener)
        {
            var type = typeof(T);

            if (!this._listeners.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                this._listeners[type] = list;
            }

            if (!list.Contains(listener))
                list.Add(listener);
        }

        public void Unsubscribe<T>(Action<T> listener)
        {
            if (this._listeners.TryGetValue(typeof(T), out var list))
                list.Remove(listener);
        }

        public void Publish<T>(T message)
        {
            if (!this._listeners.TryGetValue(typeof(T), out var list))
                return;

            foreach (var callback in list.ToArray())
                ((Action<T>)callback)?.Invoke(message);
        }
    }
}