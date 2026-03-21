using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Entities;
using ProcurementInventory.Api.Repositories;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 採購訂單 Service 實作
/// </summary>
public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _repo;

    public PurchaseOrderService(IPurchaseOrderRepository repo)
    {
        _repo = repo;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PurchaseOrderDto>> GetOrdersAsync(PurchaseOrderQueryDto query)
    {
        var orders = await _repo.GetAllAsync(query);
        return orders.Select(MapToDto);
    }

    /// <inheritdoc/>
    public async Task<PurchaseOrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _repo.GetByIdAsync(id);
        return order is null ? null : MapToDto(order);
    }

    /// <inheritdoc/>
    public async Task<PurchaseOrderDto> CreateOrderAsync(CreatePurchaseOrderDto dto, string createdBy)
    {
        // 驗證廠商必填
        if (dto.SupplierId <= 0)
            throw new ArgumentException("請選擇廠商");

        // 驗證所有明細數量 > 0
        if (dto.Items == null || !dto.Items.Any())
            throw new ArgumentException("採購數量必須大於 0");

        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException("採購數量必須大於 0");
        }

        // 產生訂單編號：PO-YYYYMMDD-NNNN
        var now = DateTime.UtcNow;
        var dateStr = now.ToString("yyyyMMdd");
        var datePrefix = $"PO-{dateStr}-";
        var maxSeq = await _repo.GetTodayMaxSequenceAsync(datePrefix);
        var nextSeq = maxSeq + 1;
        var orderNumber = $"{datePrefix}{nextSeq:D4}";

        // 計算 TotalAmount
        var totalAmount = dto.Items.Sum(i => i.Quantity * i.UnitPrice);

        var order = new PurchaseOrder
        {
            OrderNumber = orderNumber,
            SupplierId = dto.SupplierId,
            Status = "待確認",
            TotalAmount = totalAmount,
            OrderDate = now,
            CreatedAt = now,
            CreatedBy = createdBy,
            Items = dto.Items.Select(i => new PurchaseOrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Subtotal = i.Quantity * i.UnitPrice
            }).ToList()
        };

        var created = await _repo.CreateAsync(order);
        // Reload with full navigation properties
        var result = await _repo.GetByIdAsync(created.Id);
        return MapToDto(result!);
    }

    private static PurchaseOrderDto MapToDto(PurchaseOrder order) => new()
    {
        Id = order.Id,
        OrderNumber = order.OrderNumber,
        SupplierId = order.SupplierId,
        SupplierName = order.Supplier?.Name ?? string.Empty,
        Status = order.Status,
        TotalAmount = order.TotalAmount,
        OrderDate = order.OrderDate,
        CreatedAt = order.CreatedAt,
        CreatedBy = order.CreatedBy,
        Items = order.Items.Select(i => new PurchaseOrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name ?? string.Empty,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Subtotal = i.Subtotal
        }).ToList()
    };
}
