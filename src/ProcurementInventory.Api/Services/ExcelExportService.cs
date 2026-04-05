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
            ws.Cells[row, 4].Value = item.TotalWarehouseStock;
            ws.Cells[row, 5].Value = (double)item.SixMonthAvgShipment;
            ws.Cells[row, 5].Style.Numberformat.Format = "0.0";
            ws.Cells[row, 6].Value = item.MonthlyOrderSuggestions.Count > 0 ? item.MonthlyOrderSuggestions[0].SuggestedQty : 0;
            ws.Cells[row, 7].Value = item.StockStatus == "Low" ? "在庫不足" : "正常";
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

    /// <inheritdoc/>
    public byte[] ExportInventoryOverviewExtended(IEnumerable<InventoryOverviewExtendedDto> data)
    {
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("庫存總覽");

        // 標題列 - 按照 CSV 欄位順序
        var headers = new[]
        {
            "仕入先（1）", "仕入先（2）", "仕入先（3）", "仕入先（4）",
            "商品コード", "商品名", "箱入数", "MOQ",
            "リードタイム（1）", "リードタイム（2）", "リードタイム（3）", "リードタイム（4）",
            "通貨", "単価（1）", "単価（2）", "単価（3）", "単価（4）",
            "安全在庫", "平均出荷数",
            "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec",
            "89倉庫", "81倉庫", "検査倉庫", "第四倉庫", "未引当", "出荷数"
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
            int col = 1;

            // 仕入先（1）～（4）
            for (int i = 0; i < 4; i++)
            {
                ws.Cells[row, col++].Value = item.Suppliers.Count > i ? item.Suppliers[i].SupplierName : "";
            }

            // 商品コード, 商品名, 箱入数, MOQ
            ws.Cells[row, col++].Value = item.ProductCode;
            ws.Cells[row, col++].Value = item.ProductName;
            ws.Cells[row, col++].Value = item.BoxQty;
            ws.Cells[row, col++].Value = item.Moq;

            // リードタイム（1）～（4）
            for (int i = 0; i < 4; i++)
            {
                ws.Cells[row, col++].Value = item.Suppliers.Count > i ? item.Suppliers[i].LeadTimeDays : "";
            }

            // 通貨（取第一家供應商的幣別）
            ws.Cells[row, col++].Value = item.Suppliers.Count > 0 ? item.Suppliers[0].Currency : "";

            // 単価（1）～（4）
            for (int i = 0; i < 4; i++)
            {
                if (item.Suppliers.Count > i)
                {
                    ws.Cells[row, col].Value = (double)item.Suppliers[i].UnitPrice;
                    ws.Cells[row, col].Style.Numberformat.Format = "0.0000";
                }
                col++;
            }

            // 安全在庫, 平均出荷数
            ws.Cells[row, col++].Value = item.SafetyStock;
            ws.Cells[row, col].Value = (double)item.AverageShipment;
            ws.Cells[row, col].Style.Numberformat.Format = "0.0000";
            col++;

            // Jan ~ Dec
            for (int m = 1; m <= 12; m++)
            {
                ws.Cells[row, col++].Value = item.MonthlyShipments.TryGetValue(m, out var qty) ? qty : 0;
            }

            // 89倉庫, 81倉庫, 検査倉庫, 第四倉庫
            ws.Cells[row, col++].Value = item.Warehouse89;
            ws.Cells[row, col++].Value = item.Warehouse81;
            ws.Cells[row, col++].Value = item.WarehouseInspection;
            ws.Cells[row, col++].Value = item.Warehouse4th;

            // 未引当, 出荷数
            ws.Cells[row, col++].Value = item.UnallocatedQty;
            ws.Cells[row, col++].Value = item.ShippedQty;

            // 低於安全在庫標記黃色
            if (item.SafetyStock > 0 && item.TotalWarehouseStock < item.SafetyStock)
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