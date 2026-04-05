using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 月別在庫スナップショット Repository 介面
/// </summary>
public interface IMonthlyInventoryRepository
{
    /// <summary>商品IDで月別在庫を取得（直近Nヶ月）</summary>
    Task<IEnumerable<MonthlyInventory>> GetByProductIdAsync(int productId, int months = 12);

    /// <summary>月別在庫スナップショットを記録</summary>
    Task<MonthlyInventory> RecordSnapshotAsync(MonthlyInventory inventory);
}