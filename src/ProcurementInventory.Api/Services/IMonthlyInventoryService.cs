using ProcurementInventory.Api.DTOs;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 月別在庫スナップショット Service 介面
/// </summary>
public interface IMonthlyInventoryService
{
    /// <summary>商品IDで月別在庫を取得</summary>
    Task<IEnumerable<MonthlyInventoryDto>> GetMonthlyInventoryAsync(int productId, int months = 12);

    /// <summary>月別在庫スナップショットを記録</summary>
    Task<MonthlyInventoryDto> RecordMonthlySnapshotAsync(int productId, int year, int month, RecordMonthlyInventoryDto dto);
}