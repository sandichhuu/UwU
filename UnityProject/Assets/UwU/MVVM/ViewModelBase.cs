using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UwU.Events;

namespace UwU.MVVM
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected readonly EventBus EventBus = new();

        protected bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));

            return true;
        }

        protected void Publish<T>(T message)
        {
            this.EventBus.Publish(message);
        }

        public void Subscribe<T>(System.Action<T> listener)
        {
            this.EventBus.Subscribe(listener);
        }

        public void Unsubscribe<T>(System.Action<T> listener)
        {
            this.EventBus.Unsubscribe(listener);
        }
    }
}