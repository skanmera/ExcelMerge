using System;

namespace ExcelMerge.GUI.Exceptions
{
    public class ExcelMergeException : Exception
    {
        public bool ShowDialog { get; }

        public ExcelMergeException(bool showDialog, string message) : base(message)
        {
            ShowDialog = showDialog;
        }
    }
}
