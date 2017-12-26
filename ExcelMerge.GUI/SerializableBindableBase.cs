using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExcelMerge.GUI
{
    [Serializable]
    public class SerializableBindableBase : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            var old = storage;

            OnPropertyChanging(new PropertyChangedEventArgs<T>(value, old, propertyName));

            storage = value;

            RaisePropertyChanged(value, old, propertyName);

            return true;
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            var old = storage;

            OnPropertyChanging(new PropertyChangedEventArgs<T>(value, old, propertyName));

            storage = value;

            onChanged?.Invoke();
            RaisePropertyChanged(value, old, propertyName);

            return true;
        }

        protected void RaisePropertyChanged<T>(T changed, T old, [CallerMemberName]string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs<T>(changed, old, propertyName));
        }

        protected virtual void OnPropertyChanged<T>(PropertyChangedEventArgs<T> args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        protected virtual void OnPropertyChanging<T>(PropertyChangedEventArgs<T> args) { }
    }

    public class PropertyChangedEventArgs<T> : PropertyChangedEventArgs
    {
        public T ChangedValue { get; }
        public T OldValue { get; }

        public PropertyChangedEventArgs(T changed, T old, string propertyName) : base(propertyName)
        {
            ChangedValue = changed;
            OldValue = old;
        }
    }
}
