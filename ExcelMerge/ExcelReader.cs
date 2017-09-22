using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;

namespace ExcelMerge
{
    internal class ExcelReader
    {
        internal static IEnumerable<ExcelRow> Read(ISheet sheet, ExcelSheetReadConfig config)
        {
            bool skip = config.SkipFirstBlankRows;
            var actualRowIndex = 0;
            for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null)
                    continue;

                var cells = new List<ExcelCell>();
                for (int columnIndex = 0; columnIndex < row.LastCellNum; columnIndex++)
                {
                    var cell = row.GetCell(columnIndex);
                    var stringValue = ExcelUtility.GetCellStringValue(cell);

                    cells.Add(new ExcelCell(stringValue, columnIndex, rowIndex));
                }

                if (skip && !cells.Any(c => !string.IsNullOrEmpty(c.Value)))
                    continue;

                yield return new ExcelRow(actualRowIndex++, cells);
                skip = false;
            }
        }
    }
}
