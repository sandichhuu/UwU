using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UwU.MVVM
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action Disposed;

        private HideFlags m_hideFlags;
        public HideFlags HideFlags
        {
            get => this.m_hideFlags;
            set => SetProperty(ref this.m_hideFlags, value);
        }

        private Vector3 m_position;
        public Vector3 Position
        {
            get => this.m_position;
            set => SetProperty(ref this.m_position, value);
        }

        private Vector3 m_rotation;
        public Vector3 Rotation
        {
            get => this.m_rotation;
            set => SetProperty(ref this.m_rotation, value);
        }

        private Vector3 m_scale = Vector3.one;
        public Vector3 Scale
        {
            get => this.m_scale;
            set => SetProperty(ref this.m_scale, value);
        }

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

        public virtual void Dispose()
        {
            this.Disposed?.Invoke();
        }
    }
}