using ProcurementInventory.Api.DTOs;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// Excel 匯出 Service 介面
/// </summary>
public interface IExcelExportService
{
    /// <summary>
    /// 將庫存總覽資料匯出為 .xlsx 位元組陣列
    /// </summary>
    byte[] ExportInventoryOverview(IEnumerable<InventoryOverviewDto> data);

    /// <summary>
    /// 將庫存總覽資料匯出為擴展格式（包含供應商、月度出貨、倉庫資訊）
    /// </summary>
    byte[] ExportInventoryOverviewExtended(IEnumerable<InventoryOverviewExtendedDto> data);
}
