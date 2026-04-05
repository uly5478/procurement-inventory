using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Repositories;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 倉庫別在庫 Service 実装
/// </summary>
public class WarehouseStockService : IWarehouseStockService
{
    private readonly IWarehouseStockRepository _repository;

    public WarehouseStockService(IWarehouseStockRepository repository)
    {
        _repository = repository;
    }

    public async Task<WarehouseStockDto> GetWarehouseStockAsync(int productId)
    {
        var stock = await _repository.GetByProductIdAsync(productId);

        return new WarehouseStockDto
        {
            ProductId = stock.ProductId,
            Warehouse89 = stock.Warehouse89,
            Warehouse81 = stock.Warehouse81,
            WarehouseInspection = stock.WarehouseInspection,
            Warehouse4th = stock.Warehouse4th,
            TotalStock = stock.Warehouse89 + stock.Warehouse81 + stock.WarehouseInspection + stock.Warehouse4th,
            UnallocatedQty = stock.UnallocatedQty,
            ShippedQty = stock.ShippedQty,
            UpdatedAt = stock.UpdatedAt
        };
    }

    public async Task<WarehouseStockDto> UpdateWarehouseStockAsync(int productId, UpdateWarehouseStockDto dto)
    {
        var stock = await _repository.GetByProductIdAsync(productId);

        stock.Warehouse89 = dto.Warehouse89;
        stock.Warehouse81 = dto.Warehouse81;
        stock.WarehouseInspection = dto.WarehouseInspection;
        stock.Warehouse4th = dto.Warehouse4th;
        stock.UnallocatedQty = dto.UnallocatedQty;
        stock.ShippedQty = dto.ShippedQty;

        var updated = await _repository.UpdateAsync(stock);

        return new WarehouseStockDto
        {
            ProductId = updated.ProductId,
            Warehouse89 = updated.Warehouse89,
            Warehouse81 = updated.Warehouse81,
            WarehouseInspection = updated.WarehouseInspection,
            Warehouse4th = updated.Warehouse4th,
            TotalStock = updated.Warehouse89 + updated.Warehouse81 + updated.WarehouseInspection + updated.Warehouse4th,
            UnallocatedQty = updated.UnallocatedQty,
            ShippedQty = updated.ShippedQty,
            UpdatedAt = updated.UpdatedAt
        };
    }

    public async Task<int> GetTotalStockAsync(int productId)
    {
        var stock = await _repository.GetByProductIdAsync(productId);
        return stock.Warehouse89 + stock.Warehouse81 + stock.WarehouseInspection + stock.Warehouse4th;
    }
}