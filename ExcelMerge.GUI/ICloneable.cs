namespace ExcelMerge.GUI
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}
