using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Repositories;

/// <summary>
/// 採購訂單 Repository 實作
/// </summary>
public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly AppDbContext _db;

    public PurchaseOrderRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PurchaseOrder>> GetAllAsync(PurchaseOrderQueryDto query)
    {
        var q = _db.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .AsQueryable();

        if (query.StartDate.HasValue)
            q = q.Where(o => o.OrderDate >= query.StartDate.Value);

        if (query.EndDate.HasValue)
            q = q.Where(o => o.OrderDate <= query.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(query.SupplierName))
            q = q.Where(o => o.Supplier!.Name.Contains(query.SupplierName));

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(o => o.Status == query.Status);

        return await q.OrderByDescending(o => o.OrderDate).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<PurchaseOrder?> GetByIdAsync(int id)
        => await _db.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

    /// <inheritdoc/>
    public async Task<PurchaseOrder> CreateAsync(PurchaseOrder order)
    {
        _db.PurchaseOrders.Add(order);
        await _db.SaveChangesAsync();
        return order;
    }

    /// <inheritdoc/>
    public async Task<int> GetTodayMaxSequenceAsync(string datePrefix)
    {
        // datePrefix 格式：PO-YYYYMMDD-
        var orders = await _db.PurchaseOrders
            .Where(o => o.OrderNumber.StartsWith(datePrefix))
            .Select(o => o.OrderNumber)
            .ToListAsync();

        if (!orders.Any())
            return 0;

        var maxSeq = orders
            .Select(n =>
            {
                var parts = n.Split('-');
                return parts.Length == 3 && int.TryParse(parts[2], out var seq) ? seq : 0;
            })
            .Max();

        return maxSeq;
    }
}
