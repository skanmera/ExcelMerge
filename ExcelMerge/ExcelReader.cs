using System;
using System.Collections.Generic;
using NPOI.SS.UserModel;

namespace ExcelMerge
{
    internal class ExcelReader
    {
        public static IEnumerable<ExcelRow> Read(ISheet sheet)
        {
            var actualRowIndex = 0;
            for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);

                var cells = new List<ExcelCell>();
                if (row != null)
                {
                    for (int columnIndex = 0; columnIndex < row.LastCellNum; columnIndex++)
                    {
                        var cell = row.GetCell(columnIndex);
                        var stringValue = ExcelUtility.GetCellStringValue(cell);

                        cells.Add(new ExcelCell(stringValue, columnIndex, rowIndex));
                    }
                }

                yield return new ExcelRow(actualRowIndex++, cells);
            }
        }
    }
}
