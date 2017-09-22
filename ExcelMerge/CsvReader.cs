using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExcelMerge
{
    internal class CsvReader
    {
        internal static IEnumerable<ExcelRow> Read(string path, ExcelSheetReadConfig config)
        {
            using (var sr = new StreamReader(path, Encoding.UTF8))
            {
                bool skip = config.SkipFirstBlankRows;
                var rowIndex = 0;
                while (!sr.EndOfStream)
                {
                    var columnIndex = 0;
                    var cells = new List<ExcelCell>();
                    foreach (var c in sr.ReadLine().Split(','))
                        cells.Add(new ExcelCell(c, columnIndex, rowIndex));

                    if (skip && !cells.Any(c => !string.IsNullOrEmpty(c.Value)))
                        continue;

                    yield return new ExcelRow(rowIndex++, cells);
                    skip = false;
                }
            }
        }
    }
}
