using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SOSIEL_CEMMA.Output
{
    class ExcelWrite
    {
        //public String FolderName { get; private set; } = String.Concat(Algorithm._outputFolder, DateTime.Now.ToString("ddMMyy-hhmmssff"));
        //public String FolderName { get; private set; } = DateTime.Now.ToString("ddMMyy-hhmmssff");
        public String FolderName { get; set; }
        public String OutputRelationsFolderName { get; private set; } = "/OutputRelations";
        public String OutputAgentTypeFolderName { get; private set; } = "/OutputAgentType";
        public ExcelWrite(String Folder = null)
        {
            if (Folder != null)
                FolderName = Folder + "/";
            Directory.CreateDirectory(FolderName);
            Directory.CreateDirectory(FolderName + OutputRelationsFolderName);
            Directory.CreateDirectory(FolderName + OutputAgentTypeFolderName);
        }

        public void OutputStats(IEnumerable<OutputStats> outputstats)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(FolderName + $"/OutputStats.xlsx", SpreadsheetDocumentType.Workbook))
            {

                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

                FileVersion fv = new FileVersion();
                fv.ApplicationName = "Microsoft Office Excel";
                worksheetPart.Worksheet = new Worksheet(new SheetData());
                WorkbookStylesPart wbsp = workbookPart.AddNewPart<WorkbookStylesPart>();

                wbsp.Stylesheet = new Stylesheet();
                wbsp.Stylesheet.Save();


                Columns lstColumns = worksheetPart.Worksheet.GetFirstChild<Columns>();
                Boolean needToInsertColumns = false;
                if (lstColumns == null)
                {
                    lstColumns = new Columns();
                    needToInsertColumns = true;
                }

                lstColumns.Append(new Column() { Min = 1, Max = 2, Width = 20, CustomWidth = true });

                if (needToInsertColumns)
                    worksheetPart.Worksheet.InsertAt(lstColumns, 0);

                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "OutputAgentType" };
                sheets.Append(sheet);

                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                Row row = new Row();
                sheetData.Append(row);

                InsertCell(row, "Iteration", CellValues.String);
                InsertCell(row, "PropOfContribInPop", CellValues.String);
                InsertCell(row, "PropOfContripInAvePool", CellValues.String);
                InsertCell(row, "DisturbanceSize", CellValues.String);

                foreach (OutputStats item in outputstats)
                {
                    row = new Row();
                    sheetData.Append(row);
                    InsertCell(row, item.Iteration.ToString().Replace(",","."), CellValues.Number);
                    InsertCell(row, item.PropOfContribInPop.ToString().Replace(",", "."), CellValues.Number);
                    InsertCell(row, item.PropOfContripInAvePool.ToString().Replace(",", "."), CellValues.Number);
                    InsertCell(row, item.DisturbanceSize.ToString().Replace(",", "."), CellValues.Number);
                }

                workbookPart.Workbook.Save();
                document.Close();
            }
        }

        public void OutputAgentType(int iteration, IEnumerable<Agent> ActiveAgents)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(FolderName + $"/OutputAgentType/Iteration{iteration}.xlsx", SpreadsheetDocumentType.Workbook))
            {

                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

                FileVersion fv = new FileVersion();
                fv.ApplicationName = "Microsoft Office Excel";
                worksheetPart.Worksheet = new Worksheet(new SheetData());
                WorkbookStylesPart wbsp = workbookPart.AddNewPart<WorkbookStylesPart>();

                wbsp.Stylesheet = new Stylesheet();
                wbsp.Stylesheet.Save();


                Columns lstColumns = worksheetPart.Worksheet.GetFirstChild<Columns>();
                Boolean needToInsertColumns = false;
                if (lstColumns == null)
                {
                    lstColumns = new Columns();
                    needToInsertColumns = true;
                }
                lstColumns.Append(new Column() { Min = 1, Max = 2, Width = 20, CustomWidth = true });

                if (needToInsertColumns)
                    worksheetPart.Worksheet.InsertAt(lstColumns, 0);

                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "OutputAgentType" };
                sheets.Append(sheet);

                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                Row row = new Row();
                sheetData.Append(row);

                InsertCell(row, "AgentID", CellValues.String);
                InsertCell(row, "AgentType", CellValues.String);

                foreach (Agent agent in ActiveAgents)
                {
                    row = new Row();
                    sheetData.Append(row);
                    InsertCell(row, agent.Id, CellValues.String);
                    InsertCell(row, agent.Contrib ? "Sharer" : "NonSharer", CellValues.String);
                }

                workbookPart.Workbook.Save();
                document.Close();
            }
        }

        public void OutputRelations(int iteration, SocialSpace socialspace)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(FolderName + $"/OutputRelations/Iteration{iteration}.xlsx", SpreadsheetDocumentType.Workbook))
            {

                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

                FileVersion fv = new FileVersion();
                fv.ApplicationName = "Microsoft Office Excel";
                worksheetPart.Worksheet = new Worksheet(new SheetData());
                WorkbookStylesPart wbsp = workbookPart.AddNewPart<WorkbookStylesPart>();

                wbsp.Stylesheet = new Stylesheet();
                wbsp.Stylesheet.Save();

                Columns lstColumns = worksheetPart.Worksheet.GetFirstChild<Columns>();
                Boolean needToInsertColumns = false;
                if (lstColumns == null)
                {
                    lstColumns = new Columns();
                    needToInsertColumns = true;
                }
                lstColumns.Append(new Column() { Min = 1, Max = 2, Width = 20, CustomWidth = true });

                if (needToInsertColumns)
                    worksheetPart.Worksheet.InsertAt(lstColumns, 0);

                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "OutputRelations" };
                sheets.Append(sheet);

                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                Row exRow = new Row();
                sheetData.Append(exRow);

                InsertCell(exRow, "AgentID1", CellValues.String);
                InsertCell(exRow, "AgentID2", CellValues.String);

                List<(String, String)> relations = new List<(String, String)>();
                for (int row = 0; row < socialspace.Rows; row++)
                    for (int col = 0; col < socialspace.Cols; col++)
                    {
                        if (socialspace[row, col] == null)
                            continue;

                        int rowNeighbourSpot, colNeighbourSpot;

                        for (int r = row - 1; r <= row + 1; r++)
                        {

                            rowNeighbourSpot = r;

                            // if we go beyond the bounds of the array, take a spot on the other side
                            if (rowNeighbourSpot < 0)
                                rowNeighbourSpot = socialspace.Rows - 1;
                            if (rowNeighbourSpot >= socialspace.Rows)
                                rowNeighbourSpot = 0;

                            for (int c = col - 1; c <= col + 1; c++)
                            {
                                
                                colNeighbourSpot = c;

                                // if we go beyond the bounds of the array, take a spot on the other side
                                if (colNeighbourSpot < 0)
                                    colNeighbourSpot = socialspace.Cols - 1;
                                if (colNeighbourSpot >= socialspace.Cols)
                                    colNeighbourSpot = 0;

                                if (c == col && r == row)
                                    continue;

                                if (socialspace[rowNeighbourSpot, colNeighbourSpot] != null)
                                    relations.Add((socialspace[row, col].Id, socialspace[rowNeighbourSpot, colNeighbourSpot].Id));
                            }
                        }
                    }

                //IEnumerable<(String, String)> OrderResult = relations.OrderBy(e => Convert.ToInt32(e.Item1));
                IEnumerable<(String, String)> OrderResult = relations.OrderBy(e => e.Item1);
                foreach ((String, String) item in OrderResult)
                {
                    exRow = new Row();
                    sheetData.Append(exRow);
                    InsertCell(exRow, item.Item1, CellValues.String);
                    InsertCell(exRow, item.Item2, CellValues.String);
                }

                workbookPart.Workbook.Save();
                document.Close();
            }

        }

        static void InsertCell(Row row, string val, CellValues type)
        {
            Cell refCell = null;
            val = ReplaceHexadecimalSymbols(val);
            Cell newCell = new Cell();
            row.InsertBefore(newCell, refCell);

            newCell.CellValue = new CellValue(val);
            newCell.DataType = new EnumValue<CellValues>(type);

        }

        static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }
    }
}
