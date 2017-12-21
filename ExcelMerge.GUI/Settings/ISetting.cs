using System;

namespace ExcelMerge.GUI.Settings
{
    public interface ISetting<T> : IEquatable<T>
    {
        bool IsDirty { get; }

        T DeepClone();
        bool Ensure(bool isChanged = false);
    }
}
