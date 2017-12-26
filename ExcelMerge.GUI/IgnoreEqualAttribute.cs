using System;

namespace ExcelMerge.GUI
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IgnoreEqualAttribute : Attribute
    {
    }
}
