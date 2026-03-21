using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Models;
using ProcurementInventory.Api.Services;

namespace ProcurementInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// 查詢產品清單
    /// </summary>
    /// <param name="keyword">關鍵字（依產品編號或名稱模糊比對）</param>
    /// <param name="isActive">是否啟用（預設 true）</param>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProducts(
        [FromQuery] string? keyword,
        [FromQuery] bool? isActive)
    {
        // 預設只回傳啟用產品
        var activeFilter = isActive ?? true;
        var products = await _productService.GetProductsAsync(keyword, activeFilter);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(products));
    }

    /// <summary>
    /// 新增產品
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct(
        [FromBody] CreateProductDto dto)
    {
        var product = await _productService.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProducts), ApiResponse<ProductDto>.Ok(product));
    }

    /// <summary>
    /// 更新產品
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(
        int id,
        [FromBody] UpdateProductDto dto)
    {
        var product = await _productService.UpdateProductAsync(id, dto);
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    /// <summary>
    /// 停用產品
    /// </summary>
    [HttpPatch("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponse>> DeactivateProduct(int id)
    {
        await _productService.DeactivateProductAsync(id);
        return Ok(ApiResponse.OkEmpty());
    }
}
