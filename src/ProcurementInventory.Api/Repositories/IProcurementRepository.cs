using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 採購計算器 Repository 介面
/// </summary>
public interface IProcurementRepository
{
    /// <summary>取得所有採購建議（含 Product 資訊）</summary>
    Task<IEnumerable<ProcurementSuggestion>> GetAllSuggestionsAsync();

    /// <summary>依產品 Id 取得採購建議</summary>
    Task<ProcurementSuggestion?> GetSuggestionByProductIdAsync(int productId);

    /// <summary>
    /// 新增或更新採購建議（若已存在則更新，否則新增）
    /// </summary>
    Task<ProcurementSuggestion> UpsertSuggestionAsync(ProcurementSuggestion suggestion);

    /// <summary>
    /// 取得採購設定。若不存在則回傳預設值（DefaultTurnoverMonths = 2.5）
    /// </summary>
    Task<ProcurementSettings> GetSettingsAsync();

    /// <summary>更新採購設定</summary>
    Task<ProcurementSettings> UpdateSettingsAsync(ProcurementSettings settings);

    /// <summary>
    /// 取得指定產品最近 N 個月的每月出貨量。
    /// 從 StockTransaction（TransactionType="出貨"）依月份分組計算。
    /// </summary>
    Task<List<(int Year, int Month, int TotalQty)>> GetMonthlyShipmentAsync(int productId, int months);

    /// <summary>取得所有產品清單（含 InventoryRecord）</summary>
    Task<IEnumerable<Entities.Product>> GetAllProductsWithInventoryAsync();

    /// <summary>取得指定產品的當前庫存（從 InventoryRecord，若無則回傳 0）</summary>
    Task<int> GetCurrentStockAsync(int productId);
}
