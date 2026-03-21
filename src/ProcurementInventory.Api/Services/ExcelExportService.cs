using OfficeOpenXml;
using OfficeOpenXml.Style;
using ProcurementInventory.Api.DTOs;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 使用 EPPlus 實作 Excel 匯出
/// </summary>
public class ExcelExportService : IExcelExportService
{
    public ExcelExportService()
    {
        // EPPlus 5+ 需要設定授權（非商業用途）
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <inheritdoc/>
    public byte[] ExportInventoryOverview(IEnumerable<InventoryOverviewDto> data)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("庫存總覽");

        // 標題列
        var headers = new[]
        {
            "產品編號", "產品名稱", "單位", "當前庫存",
            "六個月平均出貨量", "建議採購量", "庫存狀態", "最後更新時間"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
        }

        // 資料列
        int row = 2;
        foreach (var item in data)
        {
            ws.Cells[row, 1].Value = item.ProductCode;
            ws.Cells[row, 2].Value = item.ProductName;
            ws.Cells[row, 3].Value = item.Unit;
            ws.Cells[row, 4].Value = item.CurrentStock;
            ws.Cells[row, 5].Value = (double)item.SixMonthAvgShipment;
            ws.Cells[row, 5].Style.Numberformat.Format = "0.0";
            ws.Cells[row, 6].Value = item.SuggestedProcurementQty;
            ws.Cells[row, 7].Value = item.StockStatus == "Low" ? "庫存不足" : "正常";
            ws.Cells[row, 8].Value = item.UpdatedAt.ToString("yyyy-MM-dd HH:mm");

            // 庫存不足列標記黃色
            if (item.StockStatus == "Low")
            {
                ws.Cells[row, 1, row, headers.Length].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, 1, row, headers.Length].Style.Fill.BackgroundColor
                    .SetColor(System.Drawing.Color.LightYellow);
            }

            row++;
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }
}
