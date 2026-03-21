using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 庫存 Repository 介面
/// </summary>
public interface IInventoryRepository
{
    /// <summary>依產品 Id 取得庫存記錄，若不存在則回傳 null</summary>
    Task<InventoryRecord?> GetByProductIdAsync(int productId);

    /// <summary>取得所有產品庫存記錄（含 Product 導覽屬性）</summary>
    Task<IEnumerable<InventoryRecord>> GetAllAsync();

    /// <summary>新增庫存記錄</summary>
    Task<InventoryRecord> CreateAsync(InventoryRecord record);

    /// <summary>更新庫存記錄</summary>
    Task UpdateAsync(InventoryRecord record);

    /// <summary>新增庫存異動記錄</summary>
    Task<StockTransaction> AddTransactionAsync(StockTransaction transaction);

    /// <summary>依產品 Id 查詢庫存異動歷程（依 TransactionDate 升序）</summary>
    Task<IEnumerable<StockTransaction>> GetTransactionsByProductAsync(
        int productId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>依產品 Id 查詢每月出貨統計（近 N 個月）</summary>
    Task<IEnumerable<MonthlyShipmentSummary>> GetMonthlyShipmentSummaryAsync(int productId, int months = 6);
}

/// <summary>每月出貨統計摘要</summary>
public class MonthlyShipmentSummary
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalShipped { get; set; }
}
