using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Entities;
using ProcurementInventory.Api.Repositories;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 月別在庫スナップショット Service 実装
/// </summary>
public class MonthlyInventoryService : IMonthlyInventoryService
{
    private readonly IMonthlyInventoryRepository _repository;

    public MonthlyInventoryService(IMonthlyInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<MonthlyInventoryDto>> GetMonthlyInventoryAsync(int productId, int months = 12)
    {
        var inventories = await _repository.GetByProductIdAsync(productId, months);

        return inventories.Select(i => new MonthlyInventoryDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            Year = i.Year,
            Month = i.Month,
            OrderQty = i.OrderQty,
            StockQty = i.StockQty,
            StockAmount = i.StockAmount,
            TurnoverRate = i.TurnoverRate,
            CreatedAt = i.CreatedAt
        });
    }

    public async Task<MonthlyInventoryDto> RecordMonthlySnapshotAsync(int productId, int year, int month, RecordMonthlyInventoryDto dto)
    {
        // Calculate turnover rate with zero-division handling
        decimal turnoverRate = dto.MonthlyShipmentAmount > 0
            ? dto.StockAmount / dto.MonthlyShipmentAmount
            : 0;

        var inventory = new MonthlyInventory
        {
            ProductId = productId,
            Year = year,
            Month = month,
            OrderQty = dto.OrderQty,
            StockQty = dto.StockQty,
            StockAmount = dto.StockAmount,
            TurnoverRate = turnoverRate
        };

        var result = await _repository.RecordSnapshotAsync(inventory);

        return new MonthlyInventoryDto
        {
            Id = result.Id,
            ProductId = result.ProductId,
            Year = result.Year,
            Month = result.Month,
            OrderQty = result.OrderQty,
            StockQty = result.StockQty,
            StockAmount = result.StockAmount,
            TurnoverRate = result.TurnoverRate,
            CreatedAt = result.CreatedAt
        };
    }
}