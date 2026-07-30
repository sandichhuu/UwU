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

        private readonly Dictionary<string, List<Action>> propertyBindings = new();

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

            this.propertyBindings.Clear();
            this.ViewModel = null;
        }

        protected virtual void Refresh() 
        {
            this.gameObject.hideFlags = this.ViewModel.HideFlags;
            this.transform.position = this.ViewModel.Position;
            this.transform.rotation = Quaternion.Euler(this.ViewModel.Rotation);
            this.transform.localScale = this.ViewModel.Scale;
        }

        protected virtual void RegisterBindings()
        {
            BindProperty(nameof(this.ViewModel.HideFlags), () => this.gameObject.hideFlags = this.ViewModel.HideFlags);
            BindProperty(nameof(this.ViewModel.Position), () => this.transform.position = this.ViewModel.Position);
            BindProperty(nameof(this.ViewModel.Rotation), () => this.transform.rotation = Quaternion.Euler(this.ViewModel.Rotation));
            BindProperty(nameof(this.ViewModel.Scale), () => this.transform.localScale = this.ViewModel.Scale);
        }

        protected void BindProperty(string propertyName, Action callback)
        {
            if (!this.propertyBindings.TryGetValue(propertyName, out var list))
            {
                list = new List<Action>();
                this.propertyBindings[propertyName] = list;
            }

            list.Add(callback);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.propertyBindings.TryGetValue(e.PropertyName, out var list))
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