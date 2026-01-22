using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using CadBIMHub.Models;

namespace CadBIMHub.Helpers
{
    public class ExcelHelper
    {
        private static readonly Dictionary<string, string[]> HeaderPatterns = new Dictionary<string, string[]>
        {
            { "Tên lộ", new[] { "ten", "lo", "route" } },
            { "Batch", new[] { "batch" } },
            { "Nhóm vật tư", new[] { "nhom", "group" } },
            { "Vật tư", new[] { "vat", "tu", "item" } },
            { "Kích thước", new[] { "kich", "thuoc", "size" } },
            { "Ký hiệu", new[] { "ky", "hieu", "symbol" } },
            { "Số lượng", new[] { "so", "luong", "qty", "quantity" } }
        };

        public static ExcelStructureInfoModel DetectExcelStructure(string filePath)
        {
            var result = new ExcelStructureInfoModel { IsValid = false };

            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    result.SheetName = worksheet.Name;

                    var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
                    if (lastRow == 0)
                    {
                        result.Message = "File Excel không có dữ liệu";
                        return result;
                    }

                    int headerRow = FindHeaderRow(worksheet, Math.Min(20, lastRow));
                    if (headerRow == 0)
                    {
                        result.Message = "Không tìm thấy dòng tiêu đề. Vui lòng kiểm tra file có chứa: Tên lộ, Batch, Nhóm vật tư, Vật tư, Kích thước";
                        return result;
                    }

                    result.HeaderRow = headerRow;
                    result.StartRow = headerRow + 1;
                    result.EndRow = lastRow;
                    result.IsValid = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = $"Lỗi khi đọc file: {ex.Message}";
            }

            return result;
        }

        private static int FindHeaderRow(IXLWorksheet worksheet, int maxRowToScan)
        {
            for (int row = 1; row <= maxRowToScan; row++)
            {
                var lastCol = worksheet.Row(row).LastCellUsed()?.Address.ColumnNumber ?? 0;
                if (lastCol == 0) continue;

                int matchCount = 0;
                for (int col = 1; col <= lastCol; col++)
                {
                    var cellValue = NormalizeText(worksheet.Cell(row, col).GetString());
                    if (!string.IsNullOrWhiteSpace(cellValue) && MatchesAnyPattern(cellValue))
                    {
                        matchCount++;
                    }
                }

                if (matchCount >= 3) return row;
            }
            return 0;
        }

        private static bool MatchesAnyPattern(string text)
        {
            foreach (var patterns in HeaderPatterns.Values)
            {
                if (patterns.Any(p => text.Contains(p)))
                    return true;
            }
            return false;
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            
            text = text.ToLower().Trim();
            
            var replacements = new Dictionary<string, char>
            {
                { "áàạảãâấầậẩẫăắằặẳẵ", 'a' },
                { "éèẹẻẽêếềệểễ", 'e' },
                { "óòọỏõôốồộổỗơớờợởỡ", 'o' },
                { "úùụủũưứừựửữ", 'u' },
                { "íìịỉĩ", 'i' },
                { "đ", 'd' },
                { "ýỳỵỷỹ", 'y' }
            };

            foreach (var kvp in replacements)
            {
                foreach (char c in kvp.Key)
                    text = text.Replace(c, kvp.Value);
            }

            return System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        }

        private static string GetStandardKey(string header)
        {
            var normalized = NormalizeText(header);
            
            foreach (var kvp in HeaderPatterns)
            {
                if (kvp.Value.Any(p => normalized.Contains(p)))
                    return kvp.Key;
            }
            
            return null;
        }
        public static List<ImportRouteValidationModel> ReadExcelFile(
            string filePath, 
            string sheetName, 
            int headerRow, 
            int startRow, 
            int endRow,
            List<RouteDetailModel> existingRoutes)
        {
            var results = new List<ImportRouteValidationModel>();

            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = string.IsNullOrWhiteSpace(sheetName) 
                        ? workbook.Worksheet(1) 
                        : workbook.Worksheets.FirstOrDefault(w => w.Name == sheetName);

                    if (worksheet == null)
                        throw new Exception($"Không tìm thấy trang tính '{sheetName}'");

                    var columnMapping = GetColumnMapping(worksheet, headerRow);
                    var actualEndRow = endRow > 0 ? endRow : worksheet.LastRowUsed().RowNumber();

                    for (int row = startRow; row <= actualEndRow; row++)
                    {
                        var model = new ImportRouteValidationModel
                        {
                            RouteName = GetCellValue(worksheet, row, columnMapping, "Tên lộ"),
                            BatchNo = GetCellValue(worksheet, row, columnMapping, "Batch"),
                            ItemGroup = GetCellValue(worksheet, row, columnMapping, "Nhóm vật tư"),
                            ItemDescription = GetCellValue(worksheet, row, columnMapping, "Vật tư"),
                            Size = GetCellValue(worksheet, row, columnMapping, "Kích thước"),
                            Symbol = GetCellValue(worksheet, row, columnMapping, "Ký hiệu"),
                            Quantity = GetCellValue(worksheet, row, columnMapping, "Số lượng")
                        };

                        ValidateRoute(model, existingRoutes, results);
                        results.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi đọc file Excel: {ex.Message}");
            }

            return results;
        }

        private static Dictionary<string, int> GetColumnMapping(IXLWorksheet worksheet, int headerRow)
        {
            var mapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var lastColumn = worksheet.Row(headerRow).LastCellUsed().Address.ColumnNumber;

            for (int col = 1; col <= lastColumn; col++)
            {
                var headerValue = worksheet.Cell(headerRow, col).GetString().Trim();
                if (string.IsNullOrWhiteSpace(headerValue)) continue;

                var standardKey = GetStandardKey(headerValue);
                if (!string.IsNullOrEmpty(standardKey) && !mapping.ContainsKey(standardKey))
                {
                    mapping[standardKey] = col;
                }
            }

            return mapping;
        }

        private static string GetCellValue(IXLWorksheet worksheet, int row, Dictionary<string, int> columnMapping, string columnName)
        {
            return columnMapping.ContainsKey(columnName) 
                ? worksheet.Cell(row, columnMapping[columnName]).GetString().Trim() 
                : string.Empty;
        }

        private static void ValidateRoute(ImportRouteValidationModel model, List<RouteDetailModel> existingRoutes, List<ImportRouteValidationModel> currentList)
        {
            var errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(model.RouteName))
            {
                if (existingRoutes?.Any(r => r.RouteName == model.RouteName) == true)
                    errors.Add($"Tên '{model.RouteName}' đã tồn tại trong hệ thống");
                
                if (currentList.Any(r => r.RouteName == model.RouteName))
                    errors.Add($"Tên '{model.RouteName}' đã tồn tại trong danh sách import");
            }

            if (string.IsNullOrWhiteSpace(model.ItemGroup))
                errors.Add("Thiếu dữ liệu trong cột nhóm vật tư");

            if (string.IsNullOrWhiteSpace(model.ItemDescription))
                errors.Add("Thiếu dữ liệu trong cột vật tư");

            if (string.IsNullOrWhiteSpace(model.Size))
                errors.Add("Thiếu dữ liệu cột kích thước");

            model.IsValid = errors.Count == 0;
            model.ValidationMessage = string.Join("\n", errors);
        }

        public static void ExportTemplate(string savePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Template");

                var headers = new[] { "Tên lộ", "Batch", "Nhóm vật tư", "Vật tư", "Kích thước", "Ký hiệu", "Số lượng" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }

                var sampleData = new[] { "s1", "BAT001", "Quạt gió", "Quạt hút mùi bếp", "300x200", "I", "1" };
                for (int i = 0; i < sampleData.Length; i++)
                {
                    worksheet.Cell(2, i + 1).Value = sampleData[i];
                }

                var headerRange = worksheet.Range(1, 1, 1, headers.Length);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(savePath);
            }
        }
    }
}

