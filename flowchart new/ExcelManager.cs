using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;




namespace flowchart_new
{
     class ExcelManager
    {
        //all records from excel file
        public List<Record> allRecords = new List<Record>();

        //function to read excel file. no read headers(default)
        public void readFromExcelFile(string filename, bool headers = false)
        {
            allRecords.Clear();

            //creates excel object
            var _xl = new Excel.Application();

            //open excel file
            var wb = _xl.Workbooks.Open(filename);

            //reads all sheets
            var sheets = wb.Sheets;
            if (sheets != null && sheets.Count != 0)
            {
                foreach (var item in sheets)
                {
                    //reads one sheets
                    var sheet = (Excel.Worksheet)item;
                    if (sheet != null)
                    {
                        //get column count of sheet
                        var ColumnCount = ((Excel.Range)sheet.UsedRange.Rows[1, Type.Missing]).Columns.Count;

                        //get row count of sheet
                        var rowCount = ((Excel.Range)sheet.UsedRange.Columns[1, Type.Missing]).Rows.Count;

                        // column count must bigger than 2 and row count must bigger than 1
                        if (ColumnCount < 3 || rowCount < 2)
                        {
                            continue;
                        }

                        //reads 3 of columns only
                        ColumnCount = 3;

                        int i = 0;
                        int j = 0;
                        var cell = (Excel.Range)sheet.Cells[i + 1 + (headers ? 1 : 0), j + 1];
                        if (cell.Value != "operation")
                        {
                            continue;
                        }

                        //reads all rows
                        for (i = 1; i < rowCount; i++)
                        {
                            //record object
                            Record newRecord = new Record();

                            //reads first column - operatin
                            j = 0;
                            cell = (Excel.Range)sheet.Cells[i + 1 + (headers ? 1 : 0), j + 1];
                            newRecord.operation = cell.Value;

                            if (newRecord.operation == "" || newRecord.operation == null)
                            {
                                continue;
                            }

                            //reads second column-operands
                            j = 1;
                            cell = (Excel.Range)sheet.Cells[i + 1 + (headers ? 1 : 0), j + 1];
                            newRecord.operands = cell.Value;

                            //reads third column - output
                            j = 2;
                            cell = (Excel.Range)sheet.Cells[i + 1 + (headers ? 1 : 0), j + 1];
                            newRecord.output = cell.Value;

                            //reads 4th column - line
                            j = 3;
                            cell = (Excel.Range)sheet.Cells[i + 1 + (headers ? 1 : 0), j + 1];
                            newRecord.line = (int)cell.Value;
                            allRecords.Add(newRecord);

                            if (newRecord.operation == null &&
                                newRecord.operands == null &&
                                newRecord.output == null &&
                                newRecord.line == 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            _xl.Quit();
        }
    }
}

    
