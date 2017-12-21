using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace ExcelMerge.GUI.Settings
{
    [Serializable]
    public class SettingCollection<T> : ObservableCollection<T>, IEquatable<SettingCollection<T>> where T : class
    {
        public SettingCollection() : base() { }
        public SettingCollection(IEnumerable<T> settings) : base(settings) { }

        public override bool Equals(object obj)
        {
            return Equals(obj as SettingCollection<T>);
        }

        public override int GetHashCode()
        {
            var hash = 17;

            unchecked
            {
                foreach (var item in this)
                {
                    if (item != null)
                        hash = hash * 23 + item.GetHashCode();
                }
            }

            return hash;
        }

        public bool Equals(SettingCollection<T> other)
        {
            if (other == null)
                return false;

            return this.SequenceEqual(other);
        }
    }
}
