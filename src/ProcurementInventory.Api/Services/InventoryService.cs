using ProcurementInventory.Api.DTOs;
using ProcurementInventory.Api.Entities;
using ProcurementInventory.Api.Repositories;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 庫存管理 Service 實作
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repo;
    private readonly IProcurementRepository _procurementRepo;

    public InventoryService(IInventoryRepository repo, IProcurementRepository procurementRepo)
    {
        _repo = repo;
        _procurementRepo = procurementRepo;
    }

    /// <inheritdoc/>
    public async Task<StockTransactionResultDto> StockInAsync(StockInDto dto, string operatorAccount)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("入庫數量必須大於 0");

        var record = await _repo.GetByProductIdAsync(dto.ProductId);
        int stockBefore;

        if (record is null)
        {
            // 首次入庫，建立庫存記錄
            stockBefore = 0;
            record = new InventoryRecord
            {
                ProductId = dto.ProductId,
                CurrentStock = dto.Quantity,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.CreateAsync(record);
        }
        else
        {
            stockBefore = record.CurrentStock;
            record.CurrentStock += dto.Quantity;
            record.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(record);
        }

        var transaction = new StockTransaction
        {
            ProductId = dto.ProductId,
            TransactionType = "入庫",
            Quantity = dto.Quantity,
            StockBefore = stockBefore,
            StockAfter = record.CurrentStock,
            PurchaseOrderId = dto.PurchaseOrderId,
            TransactionDate = dto.TransactionDate,
            OperatorAccount = operatorAccount,
            CreatedAt = DateTime.UtcNow,
            Remark = dto.Remark
        };

        var saved = await _repo.AddTransactionAsync(transaction);

        return new StockTransactionResultDto
        {
            TransactionId = saved.Id,
            ProductId = dto.ProductId,
            ProductName = record.Product?.Name ?? string.Empty,
            TransactionType = "入庫",
            Quantity = dto.Quantity,
            StockBefore = stockBefore,
            StockAfter = record.CurrentStock,
            TransactionDate = dto.TransactionDate,
            OperatorAccount = operatorAccount,
            Remark = dto.Remark
        };
    }

    /// <inheritdoc/>
    public async Task<StockTransactionResultDto> StockOutAsync(StockOutDto dto, string operatorAccount)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("出貨數量必須大於 0");

        var record = await _repo.GetByProductIdAsync(dto.ProductId);
        int currentStock = record?.CurrentStock ?? 0;

        // 超庫存警告
        if (dto.Quantity > currentStock && !dto.ForceConfirm)
        {
            return new StockTransactionResultDto
            {
                ProductId = dto.ProductId,
                TransactionType = "出貨",
                Quantity = dto.Quantity,
                StockBefore = currentStock,
                StockAfter = currentStock,
                Warning = $"出貨數量（{dto.Quantity}）超過當前庫存（{currentStock}），是否確認執行？",
                RequireConfirmation = true
            };
        }

        int stockBefore = currentStock;
        int stockAfter = currentStock - dto.Quantity;

        if (record is null)
        {
            record = new InventoryRecord
            {
                ProductId = dto.ProductId,
                CurrentStock = stockAfter,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.CreateAsync(record);
        }
        else
        {
            record.CurrentStock = stockAfter;
            record.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(record);
        }

        var transaction = new StockTransaction
        {
            ProductId = dto.ProductId,
            TransactionType = "出貨",
            Quantity = dto.Quantity,
            StockBefore = stockBefore,
            StockAfter = stockAfter,
            TransactionDate = dto.TransactionDate,
            OperatorAccount = operatorAccount,
            CreatedAt = DateTime.UtcNow,
            Remark = dto.Remark
        };

        var saved = await _repo.AddTransactionAsync(transaction);

        return new StockTransactionResultDto
        {
            TransactionId = saved.Id,
            ProductId = dto.ProductId,
            ProductName = record.Product?.Name ?? string.Empty,
            TransactionType = "出貨",
            Quantity = dto.Quantity,
            StockBefore = stockBefore,
            StockAfter = stockAfter,
            TransactionDate = dto.TransactionDate,
            OperatorAccount = operatorAccount,
            Remark = dto.Remark
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<StockTransactionDto>> GetTransactionHistoryAsync(
        int productId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var transactions = await _repo.GetTransactionsByProductAsync(productId, startDate, endDate);
        return transactions.Select(t => new StockTransactionDto
        {
            Id = t.Id,
            ProductId = t.ProductId,
            TransactionType = t.TransactionType,
            Quantity = t.Quantity,
            StockBefore = t.StockBefore,
            StockAfter = t.StockAfter,
            PurchaseOrderId = t.PurchaseOrderId,
            TransactionDate = t.TransactionDate,
            OperatorAccount = t.OperatorAccount,
            CreatedAt = t.CreatedAt,
            Remark = t.Remark
        });
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MonthlyShipmentDto>> GetMonthlyShipmentSummaryAsync(
        int productId, int months = 6)
    {
        var summaries = await _repo.GetMonthlyShipmentSummaryAsync(productId, months);
        return summaries.Select(s => new MonthlyShipmentDto
        {
            Year = s.Year,
            Month = s.Month,
            TotalShipped = s.TotalShipped
        });
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InventoryOverviewDto>> GetInventoryOverviewAsync()
    {
        var records = await _repo.GetAllAsync();
        var result = new List<InventoryOverviewDto>();

        foreach (var record in records)
        {
            // 計算六個月平均出貨量
            var monthlySummaries = await _repo.GetMonthlyShipmentSummaryAsync(record.ProductId, 6);
            var summaryList = monthlySummaries.ToList();
            decimal avgShipment = summaryList.Count > 0
                ? (decimal)summaryList.Sum(s => s.TotalShipped) / summaryList.Count
                : 0;

            // 取得採購建議量（若有）
            var suggestion = await _procurementRepo.GetSuggestionByProductIdAsync(record.ProductId);
            int suggestedQty = suggestion?.SystemSuggestedQty ?? 0;

            // 庫存狀態：低於六個月平均出貨量 → Low
            string status = record.CurrentStock < (int)avgShipment ? "Low" : "Normal";

            result.Add(new InventoryOverviewDto
            {
                ProductId = record.ProductId,
                ProductCode = record.Product?.ProductCode ?? string.Empty,
                ProductName = record.Product?.Name ?? string.Empty,
                Unit = record.Product?.Unit ?? string.Empty,
                CurrentStock = record.CurrentStock,
                SixMonthAvgShipment = avgShipment,
                SuggestedProcurementQty = suggestedQty,
                StockStatus = status,
                UpdatedAt = record.UpdatedAt
            });
        }

        return result;
    }
}
