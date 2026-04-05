using ProcurementInventory.Api.DTOs;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 倉庫別在庫 Service 介面
/// </summary>
public interface IWarehouseStockService
{
    /// <summary>商品IDで倉庫別在庫を取得</summary>
    Task<WarehouseStockDto> GetWarehouseStockAsync(int productId);

    /// <summary>倉庫別在庫を更新</summary>
    Task<WarehouseStockDto> UpdateWarehouseStockAsync(int productId, UpdateWarehouseStockDto dto);

    /// <summary>総在庫数量を取得（4倉庫の合計）</summary>
    Task<int> GetTotalStockAsync(int productId);
}