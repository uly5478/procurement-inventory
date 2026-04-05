using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Models;
using ProcurementInventory.Api.Services;

namespace ProcurementInventory.Api.Controllers;

[ApiController]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly AppDbContext _db;

    public SuppliersController(ISupplierService supplierService, AppDbContext db)
    {
        _supplierService = supplierService;
        _db = db;
    }

    /// <summary>全仕入先一覧</summary>
    [HttpGet("api/suppliers")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetAllSuppliers()
    {
        var suppliers = await _db.Suppliers
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync();
        return Ok(ApiResponse<IEnumerable<object>>.Ok(suppliers));
    }

    /// <summary>仕入先別の商品・発注数プレビュー（調達提案ベース）</summary>
    [HttpGet("api/suppliers/{supplierId:int}/order-preview")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetOrderPreview(int supplierId)
    {
        // この仕入先が担当する商品と最安値単価を取得
        var prices = await _db.ProductSupplierPrices
            .Include(p => p.Product)
            .Where(p => p.SupplierId == supplierId && p.IsCurrent)
            .OrderBy(p => p.Product!.ProductCode)
            .ToListAsync();

        var result = prices.Select(p => new
        {
            productId = p.ProductId,
            productCode = p.Product?.ProductCode ?? "",
            productName = p.Product?.Name ?? "",
            unitPrice = p.UnitPrice,
            currency = p.Currency,
            leadTimeDays = p.LeadTimeDays,
            moq = p.Product?.MOQ ?? 1,
            boxQty = p.Product?.BoxQty ?? 1,
            averageShipment = p.Product?.AverageShipment ?? 0,
            safetyStock = p.Product?.SafetyStock ?? 0,
        });

        return Ok(ApiResponse<IEnumerable<object>>.Ok(result));
    }

    /// <summary>
    /// 取得指定產品的廠商報價清單（依買價升序）
    /// 需求 2.5
    /// </summary>
    [HttpGet("api/products/{productId:int}/suppliers")]
    public async Task<ActionResult<ApiResponse<SupplierPriceListResult>>> GetProductSuppliers(int productId)
    {
        var result = await _supplierService.GetProductSuppliersAsync(productId);
        return Ok(ApiResponse<SupplierPriceListResult>.Ok(result));
    }

    /// <summary>
    /// 新增廠商報價。
    /// 若已有 4 家廠商且 ForceCreate=false，回傳 200 + RequireConfirmation=true。
    /// 成功新增時回傳 201 Created。
    /// 需求 2.1, 2.2, 2.4
    /// </summary>
    [HttpPost("api/products/{productId:int}/suppliers")]
    public async Task<ActionResult<ApiResponse<SupplierPriceListResult>>> AddSupplierPrice(
        int productId,
        [FromBody] CreateSupplierPriceDto dto)
    {
        var result = await _supplierService.AddSupplierPriceAsync(productId, dto);

        if (result.RequireConfirmation)
        {
            // 需求 2.4：第 5 家廠商警告，回傳 200 + warning
            return Ok(ApiResponse<SupplierPriceListResult>.Ok(result));
        }

        // 成功新增，回傳 201 Created
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SupplierPriceListResult>.Ok(result));
    }

    /// <summary>
    /// 更新廠商報價（保留歷史記錄）
    /// 需求 2.3
    /// </summary>
    [HttpPut("api/suppliers/{priceId:int}")]
    public async Task<ActionResult<ApiResponse<SupplierPriceDto>>> UpdateSupplierPrice(
        int priceId,
        [FromBody] UpdateSupplierPriceDto dto)
    {
        var result = await _supplierService.UpdateSupplierPriceAsync(priceId, dto);
        return Ok(ApiResponse<SupplierPriceDto>.Ok(result));
    }
}
