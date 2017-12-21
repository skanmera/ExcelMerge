using System;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;
using SKCore.Runtime.Serialization;

namespace ExcelMerge.GUI.Settings
{
    [Serializable]
    public class Setting<T> : SerializableBindableBase, ISetting<T> where T : Setting<T>
    {
        [YamlIgnore, IgnoreEqual]
        public Setting<T> PreviousSetting { get; protected set; }

        [NonSerialized]
        protected bool isDirty;
        [YamlIgnore, IgnoreEqual]
        public bool IsDirty
        {
            get { return isDirty; }
            protected set { SetProperty(ref isDirty, value); }
        }

        public Setting()
        {
            PreviousSetting = DeepClone();
        }

        public virtual T DeepClone()
        {
            return SerializationUtility.DeepClone(this as T);
        }

        public virtual bool Ensure(bool isChanged = false)
        {
            return isChanged;
        }

        public bool Equals(T other)
        {
            if (other == null)
                return false;

            var properties = GetType().GetProperties().Where(p => !p.IsDefined(typeof(IgnoreEqualAttribute)));
            foreach (var property in properties)
            {
                var selfValue = property.GetValue(this);
                var otherValue = property.GetValue(other);

                if ((selfValue == null) != (otherValue == null))
                    return false;

                if (selfValue == null && otherValue == null)
                    continue;

                if (!selfValue.Equals(otherValue))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var properties = GetType().GetProperties().Where(p => !p.IsDefined(typeof(IgnoreEqualAttribute)));
            int hash = 17;

            unchecked
            {
                foreach (var property in properties)
                {
                    var value = property.GetValue(this);
                    if (value != null)
                        hash = hash * 23 + value.GetHashCode();
                }
            }

            return hash;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as T);
        }

        public virtual void Clean()
        {
            PreviousSetting = DeepClone();

            IsDirty = false;
        }

        protected override void OnPropertyChanging<TValue>(PropertyChangedEventArgs<TValue> args)
        {
            base.OnPropertyChanging(args);
        }

        protected override void OnPropertyChanged<TValue>(PropertyChangedEventArgs<TValue> args)
        {
            base.OnPropertyChanged(args);

            if (args.PropertyName != nameof(IsDirty))
                IsDirty = !Equals(PreviousSetting);
        }
    }
}
