using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 月別出荷実績 Repository 実装
/// </summary>
public class MonthlyShipmentRepository : IMonthlyShipmentRepository
{
    private readonly AppDbContext _context;

    public MonthlyShipmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MonthlyShipment>> GetByProductIdAsync(int productId, int? year = null)
    {
        var query = _context.MonthlyShipments
            .Where(m => m.ProductId == productId);

        if (year.HasValue)
        {
            query = query.Where(m => m.Year == year.Value);
        }

        return await query.OrderBy(m => m.Year).ThenBy(m => m.Month).ToListAsync();
    }

    public async Task<MonthlyShipment> UpsertAsync(MonthlyShipment shipment)
    {
        var existing = await _context.MonthlyShipments
            .FirstOrDefaultAsync(m => m.ProductId == shipment.ProductId 
                && m.Year == shipment.Year 
                && m.Month == shipment.Month);

        if (existing != null)
        {
            existing.Quantity = shipment.Quantity;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.MonthlyShipments.Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        shipment.CreatedAt = DateTime.UtcNow;
        _context.MonthlyShipments.Add(shipment);
        await _context.SaveChangesAsync();
        return shipment;
    }

    public async Task<IEnumerable<MonthlyShipment>> BulkUpsertAsync(int productId, int year, Dictionary<int, int> monthQuantities)
    {
        var results = new List<MonthlyShipment>();

        foreach (var (month, quantity) in monthQuantities)
        {
            if (month < 1 || month > 12) continue;

            var existing = await _context.MonthlyShipments
                .FirstOrDefaultAsync(m => m.ProductId == productId 
                    && m.Year == year 
                    && m.Month == month);

            if (existing != null)
            {
                existing.Quantity = quantity;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.MonthlyShipments.Update(existing);
                results.Add(existing);
            }
            else
            {
                var shipment = new MonthlyShipment
                {
                    ProductId = productId,
                    Year = year,
                    Month = month,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow
                };
                _context.MonthlyShipments.Add(shipment);
                results.Add(shipment);
            }
        }

        await _context.SaveChangesAsync();
        return results;
    }
}