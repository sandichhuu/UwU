using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace UwU.MVVM
{
    public abstract class ViewBase<T> : MonoBehaviour
        where T : ViewModelBase
    {
        public T ViewModel { get; private set; }

        private readonly Dictionary<string, List<Action>> _propertyBindings = new();

        private readonly List<Action> _unsubscribeEvents = new();

        public void Bind(T vm)
        {
            Unbind();

            this.ViewModel = vm;

            RegisterBindings();

            this.ViewModel.PropertyChanged += OnPropertyChanged;

            Refresh();
        }

        public void Unbind()
        {
            if (this.ViewModel == null)
                return;

            this.ViewModel.PropertyChanged -= OnPropertyChanged;

            foreach (var dispose in this._unsubscribeEvents)
                dispose();

            this._unsubscribeEvents.Clear();

            this._propertyBindings.Clear();

            this.ViewModel = null;
        }

        protected virtual void Refresh() { }

        protected abstract void RegisterBindings();

        protected void BindProperty(string propertyName, Action callback)
        {
            if (!this._propertyBindings.TryGetValue(propertyName, out var list))
            {
                list = new List<Action>();
                this._propertyBindings[propertyName] = list;
            }

            list.Add(callback);
        }

        protected void BindEvent<TEvent>(Action<TEvent> callback)
        {
            this.ViewModel.Subscribe(callback);

            this._unsubscribeEvents.Add(() =>
                this.ViewModel.Unsubscribe(callback));
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this._propertyBindings.TryGetValue(e.PropertyName, out var list))
            {
                foreach (var action in list)
                    action();
            }
        }

        protected virtual void OnDestroy()
        {
            Unbind();
        }
    }
}