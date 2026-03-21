using ProcurementInventory.Api.DTOs;

namespace ProcurementInventory.Api.Services;

/// <summary>
/// 採購建議量計算 Service 介面
/// </summary>
public interface IProcurementCalculatorService
{
    /// <summary>
    /// 取得所有產品的採購建議清單。
    /// useForecast=true 時使用需求預測值取代六個月平均出貨量（需求 9.6）。
    /// </summary>
    Task<IEnumerable<ProcurementSuggestionDto>> GetAllSuggestionsAsync(bool useForecast = false);

    /// <summary>取得單一產品的採購建議</summary>
    Task<ProcurementSuggestionDto?> GetSuggestionByProductIdAsync(int productId);

    /// <summary>
    /// 手動覆寫指定產品的採購量。
    /// 儲存 ManualOverrideQty 並設定 IsManualOverride = true。
    /// </summary>
    Task<ProcurementSuggestionDto> ManualOverrideAsync(int productId, int qty);

    /// <summary>取得採購設定</summary>
    Task<ProcurementSettingsDto> GetSettingsAsync();

    /// <summary>
    /// 更新採購設定。
    /// DefaultTurnoverMonths 必須介於 1.0–6.0，否則拋出 ArgumentException。
    /// </summary>
    Task<ProcurementSettingsDto> UpdateSettingsAsync(UpdateProcurementSettingsDto dto);
}
