using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 倉庫別在庫 Repository 介面
/// </summary>
public interface IWarehouseStockRepository
{
    /// <summary>商品IDで倉庫別在庫を取得（存在しない場合は自動作成）</summary>
    Task<WarehouseStock> GetByProductIdAsync(int productId);

    /// <summary>倉庫別在庫を更新</summary>
    Task<WarehouseStock> UpdateAsync(WarehouseStock stock);
}