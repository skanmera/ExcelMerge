using System.Collections.Generic;
using NPOI.SS.UserModel;

namespace ExcelMerge
{
    public class ExcelWorkbook
    {
        public Dictionary<string, ExcelSheet> Sheets { get; private set; }

        public ExcelWorkbook()
        {
            Sheets = new Dictionary<string, ExcelSheet>();
        }

        public static ExcelWorkbook Create(string path, ExcelSheetReadConfig config)
        {
            if (System.IO.Path.GetExtension(path) == ".csv")
                return CreateFromCsv(path, config);

            var srcWb = WorkbookFactory.Create(path);
            var wb = new ExcelWorkbook();
            for (int i = 0; i < srcWb.NumberOfSheets; i++)
            {
                var srcSheet = srcWb.GetSheetAt(i);
                wb.Sheets.Add(srcSheet.SheetName, ExcelSheet.Create(srcSheet, config));
            }

            return wb;
        }

        private static ExcelWorkbook CreateFromCsv(string path, ExcelSheetReadConfig config)
        {
            var wb = new ExcelWorkbook();
            wb.Sheets.Add("csv", ExcelSheet.CreateFromCsv(path, config));

            return wb;
        }
    }
}
