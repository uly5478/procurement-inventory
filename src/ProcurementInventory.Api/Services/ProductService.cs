using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Entities;
using ProcurementInventory.Api.Repositories;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 產品 Service 實作
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProductDto>> GetProductsAsync(string? keyword, bool? isActive)
    {
        var products = await _repo.GetAllAsync(keyword, isActive);
        return products.Select(ToDto);
    }

    /// <inheritdoc/>
    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        return product is null ? null : ToDto(product);
    }

    /// <inheritdoc/>
    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        // 需求 1.3：產品編號唯一性驗證
        if (await _repo.ExistsAsync(dto.ProductCode))
            throw new ArgumentException("產品編號已存在，請使用不同的產品編號");

        var product = new Product
        {
            ProductCode = dto.ProductCode,
            Name = dto.Name,
            Unit = dto.Unit,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(product);
        return ToDto(created);
    }

    /// <inheritdoc/>
    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"找不到 Id 為 {id} 的產品");

        product.Name = dto.Name;
        product.Unit = dto.Unit;
        // 需求 1.4：記錄最後修改時間
        product.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.UpdateAsync(product);
        return ToDto(updated);
    }

    /// <inheritdoc/>
    public async Task DeactivateProductAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"找不到 Id 為 {id} 的產品");

        // 需求 1.5：停用產品
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(product);
    }

    private static ProductDto ToDto(Product p) => new()
    {
        Id = p.Id,
        ProductCode = p.ProductCode,
        Name = p.Name,
        Unit = p.Unit,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
