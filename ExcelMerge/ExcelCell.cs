namespace ExcelMerge
{
    public class ExcelCell
    {
        public string Value { get; private set; }
        public int OriginalColumnIndex { get; private set; }
        public int OriginalRowIndex { get; private set; }

        public ExcelCell(string value, int originalColumnIndex, int originalRowIndex)
        {
            Value = value;
            OriginalColumnIndex = originalColumnIndex;
            OriginalRowIndex = originalRowIndex;
        }
    }
}
