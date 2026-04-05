using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 月別出荷実績 Repository 介面
/// </summary>
public interface IMonthlyShipmentRepository
{
    /// <summary>商品IDで月別出荷実績を取得</summary>
    Task<IEnumerable<MonthlyShipment>> GetByProductIdAsync(int productId, int? year = null);

    /// <summary>単一レコードを登録または更新</summary>
    Task<MonthlyShipment> UpsertAsync(MonthlyShipment shipment);

    /// <summary>一括登録・更新（12ヶ月分）</summary>
    Task<IEnumerable<MonthlyShipment>> BulkUpsertAsync(int productId, int year, Dictionary<int, int> monthQuantities);
}