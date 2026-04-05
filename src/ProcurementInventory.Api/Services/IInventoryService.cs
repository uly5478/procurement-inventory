using ProcurementInventory.Api.DTOs;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 庫存管理 Service 介面
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// 入庫操作：建立 StockTransaction、更新 InventoryRecord.CurrentStock
    /// </summary>
    Task<StockTransactionResultDto> StockInAsync(StockInDto dto, string operatorAccount);

    /// <summary>
    /// 出貨操作：超庫存時回傳警告 + requireConfirmation，
    /// 確認後（forceConfirm=true）執行扣減
    /// </summary>
    Task<StockTransactionResultDto> StockOutAsync(StockOutDto dto, string operatorAccount);

    /// <summary>
    /// 查詢指定產品的庫存異動歷程
    /// </summary>
    Task<IEnumerable<StockTransactionDto>> GetTransactionHistoryAsync(
        int productId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// 查詢指定產品的每月出貨統計
    /// </summary>
    Task<IEnumerable<MonthlyShipmentDto>> GetMonthlyShipmentSummaryAsync(int productId, int months = 6);

    /// <summary>
    /// 取得所有產品庫存總覽（含六個月平均出貨量、建議採購量、庫存狀態）
    /// </summary>
    Task<IEnumerable<InventoryOverviewDto>> GetInventoryOverviewAsync();

    /// <summary>
    /// 取得所有產品庫存總覽擴展格式（含供應商、月度出貨、倉庫資訊）
    /// </summary>
    Task<IEnumerable<InventoryOverviewExtendedDto>> GetInventoryOverviewExtendedAsync();
}
