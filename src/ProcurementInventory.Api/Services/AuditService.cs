using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProcurementInventory.Api.Data;
using ProcurementInventory.Api.Entities;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 帳實相符稽核 Service（需求 8.3, 8.4）
/// </summary>
public class AuditService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext db, ILogger<AuditService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 對所有產品執行帳實核對。
    /// 將 StockTransaction 依時間累計後與 InventoryRecord.CurrentStock 比對，
    /// 若不一致則寫入 AuditDiscrepancyLog。
    /// </summary>
    public async Task<IEnumerable<AuditDiscrepancyLog>> RunAuditAsync()
    {
        var discrepancies = new List<AuditDiscrepancyLog>();

        var products = await _db.Products.Where(p => p.IsActive).ToListAsync();

        foreach (var product in products)
        {
            // 帳面庫存：累計所有 StockTransaction
            var transactions = await _db.StockTransactions
                .Where(t => t.ProductId == product.Id)
                .ToListAsync();

            int bookStock = transactions.Sum(t =>
                t.TransactionType == "入庫" ? t.Quantity : -t.Quantity);

            // 實際庫存：InventoryRecord.CurrentStock
            var record = await _db.InventoryRecords
                .FirstOrDefaultAsync(r => r.ProductId == product.Id);
            int actualStock = record?.CurrentStock ?? 0;

            if (bookStock != actualStock)
            {
                _logger.LogWarning(
                    "帳實不符：產品 {ProductId} ({ProductCode})，帳面 {BookStock}，實際 {ActualStock}",
                    product.Id, product.ProductCode, bookStock, actualStock);

                var log = new AuditDiscrepancyLog
                {
                    ProductId = product.Id,
                    BookStock = bookStock,
                    ActualStock = actualStock,
                    Discrepancy = bookStock - actualStock,
                    AuditedAt = DateTime.UtcNow,
                    NotificationSent = false
                };

                _db.AuditDiscrepancyLogs.Add(log);
                discrepancies.Add(log);
            }
        }

        if (discrepancies.Count > 0)
            await _db.SaveChangesAsync();

        return discrepancies;
    }
}

/// <summary>
/// 每日凌晨 2:00 執行帳實核對的背景服務（需求 8.4）
/// </summary>
public class AuditBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditBackgroundService> _logger;

    public AuditBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AuditBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 計算距離下一個凌晨 2:00 的等待時間
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(now.Hour >= 2 ? 1 : 0).AddHours(2);
            var delay = nextRun - now;

            _logger.LogInformation("下次帳實核對排程：{NextRun}", nextRun);
            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var auditService = scope.ServiceProvider.GetRequiredService<AuditService>();
                var discrepancies = await auditService.RunAuditAsync();
                _logger.LogInformation("帳實核對完成，發現 {Count} 筆不符記錄", discrepancies.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "帳實核對執行失敗");
            }
        }
    }
}
