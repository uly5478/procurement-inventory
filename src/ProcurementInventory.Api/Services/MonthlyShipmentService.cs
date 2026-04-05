using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Entities;
using ProcurementInventory.Api.Repositories;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 月別出荷実績 Service 実装
/// </summary>
public class MonthlyShipmentService : IMonthlyShipmentService
{
    private readonly IMonthlyShipmentRepository _repository;

    public MonthlyShipmentService(IMonthlyShipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<MonthlyShipmentResultDto?> GetMonthlyShipmentsAsync(int productId, int year)
    {
        var shipments = await _repository.GetByProductIdAsync(productId, year);

        if (!shipments.Any())
        {
            return new MonthlyShipmentResultDto { Year = year };
        }

        var result = new MonthlyShipmentResultDto { Year = year };

        foreach (var s in shipments.Where(s => s.Year == year))
        {
            switch (s.Month)
            {
                case 1: result.Jan = s.Quantity; break;
                case 2: result.Feb = s.Quantity; break;
                case 3: result.Mar = s.Quantity; break;
                case 4: result.Apr = s.Quantity; break;
                case 5: result.May = s.Quantity; break;
                case 6: result.Jun = s.Quantity; break;
                case 7: result.Jul = s.Quantity; break;
                case 8: result.Aug = s.Quantity; break;
                case 9: result.Sep = s.Quantity; break;
                case 10: result.Oct = s.Quantity; break;
                case 11: result.Nov = s.Quantity; break;
                case 12: result.Dec = s.Quantity; break;
            }
        }

        return result;
    }

    public async Task<MonthlyShipmentRecordDto> UpsertMonthlyShipmentAsync(int productId, UpsertMonthlyShipmentDto dto)
    {
        var shipment = new MonthlyShipment
        {
            ProductId = productId,
            Year = dto.Year,
            Month = dto.Month,
            Quantity = dto.Quantity
        };

        var result = await _repository.UpsertAsync(shipment);

        return new MonthlyShipmentRecordDto
        {
            Id = result.Id,
            ProductId = result.ProductId,
            Year = result.Year,
            Month = result.Month,
            Quantity = result.Quantity
        };
    }

    public async Task<IEnumerable<MonthlyShipmentRecordDto>> BulkUpsertMonthlyShipmentsAsync(int productId, BulkUpsertMonthlyShipmentsDto dto)
    {
        var results = await _repository.BulkUpsertAsync(productId, dto.Year, dto.MonthQuantities);

        return results.Select(r => new MonthlyShipmentRecordDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            Year = r.Year,
            Month = r.Month,
            Quantity = r.Quantity
        });
    }
}