using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Entities;
using ProcurementInventory.Api.Repositories;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 廠商報價 Service 實作
/// </summary>
public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repo;

    public SupplierService(ISupplierRepository repo)
    {
        _repo = repo;
    }

    /// <inheritdoc/>
    public async Task<SupplierPriceListResult> GetProductSuppliersAsync(int productId)
    {
        // 需求 2.5：依買價由低至高排序
        var prices = await _repo.GetCurrentPricesByProductIdAsync(productId);
        return new SupplierPriceListResult
        {
            Items = prices.Select(ToDto).ToList(),
            RequireConfirmation = false
        };
    }

    /// <inheritdoc/>
    public async Task<SupplierPriceListResult> AddSupplierPriceAsync(int productId, CreateSupplierPriceDto dto)
    {
        // 需求 2.4：第 5 家廠商警告邏輯
        var currentCount = await _repo.GetCurrentSupplierCountAsync(productId);
        if (currentCount >= 4 && !dto.ForceCreate)
        {
            return new SupplierPriceListResult
            {
                Items = new List<SupplierPriceDto>(),
                Warning = "已達廠商數量上限（4 家），是否確認繼續？",
                RequireConfirmation = true
            };
        }

        // 查詢或建立廠商
        var supplier = await _repo.GetSupplierByNameAsync(dto.SupplierName)
            ?? await _repo.CreateSupplierAsync(new Supplier { Name = dto.SupplierName });

        // 需求 2.2：儲存廠商名稱、產品買價、幣別、最小訂購量及交期
        var price = new ProductSupplierPrice
        {
            ProductId = productId,
            SupplierId = supplier.Id,
            UnitPrice = dto.UnitPrice,
            Currency = dto.Currency,
            MinOrderQty = dto.MinOrderQty,
            LeadTimeDays = dto.LeadTimeDays,
            EffectiveDate = DateTime.UtcNow,
            IsCurrent = true
        };

        var created = await _repo.CreatePriceAsync(price);
        created.Supplier = supplier;

        // 回傳更新後的完整清單
        var allCurrent = await _repo.GetCurrentPricesByProductIdAsync(productId);
        return new SupplierPriceListResult
        {
            Items = allCurrent.Select(ToDto).ToList(),
            RequireConfirmation = false
        };
    }

    /// <inheritdoc/>
    public async Task<SupplierPriceDto> UpdateSupplierPriceAsync(int priceId, UpdateSupplierPriceDto dto)
    {
        var oldPrice = await _repo.GetPriceByIdAsync(priceId)
            ?? throw new KeyNotFoundException($"找不到 Id 為 {priceId} 的報價記錄");

        // 需求 2.3：將舊報價 IsCurrent 設為 false（保留歷史記錄）
        oldPrice.IsCurrent = false;
        await _repo.UpdatePriceAsync(oldPrice);

        // 建立新報價記錄（IsCurrent=true）
        var newPrice = new ProductSupplierPrice
        {
            ProductId = oldPrice.ProductId,
            SupplierId = oldPrice.SupplierId,
            UnitPrice = dto.UnitPrice,
            Currency = dto.Currency,
            MinOrderQty = dto.MinOrderQty,
            LeadTimeDays = dto.LeadTimeDays,
            EffectiveDate = DateTime.UtcNow,
            IsCurrent = true
        };

        var created = await _repo.CreatePriceAsync(newPrice);

        // 載入廠商資訊
        var supplier = oldPrice.Supplier;
        created.Supplier = supplier;

        return ToDto(created);
    }

    /// <inheritdoc/>
    public async Task<SupplierPriceDto?> GetRecommendedSupplierAsync(int productId)
    {
        var prices = await _repo.GetCurrentPricesByProductIdAsync(productId);
        var recommended = prices.OrderBy(p => p.UnitPrice).FirstOrDefault();
        
        return recommended == null ? null : ToDto(recommended);
    }

    private static SupplierPriceDto ToDto(ProductSupplierPrice p) => new()
    {
        Id = p.Id,
        ProductId = p.ProductId,
        SupplierId = p.SupplierId,
        SupplierName = p.Supplier?.Name ?? string.Empty,
        UnitPrice = p.UnitPrice,
        Currency = p.Currency,
        MinOrderQty = p.MinOrderQty,
        LeadTimeDays = p.LeadTimeDays,
        EffectiveDate = p.EffectiveDate,
        IsCurrent = p.IsCurrent
    };
}
