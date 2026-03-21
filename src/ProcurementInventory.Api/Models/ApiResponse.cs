namespace ProcurementInventory.Api.Models;

/// <summary>
/// 統一 API 回應格式
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public string? RequestId { get; set; }

    public static ApiResponse<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string error, string? requestId = null) =>
        new() { Success = false, Error = error, RequestId = requestId };
}

/// <summary>
/// 業務邏輯警告回應（HTTP 200 + 警告旗標）
/// </summary>
public class WarningResponse
{
    public bool Success { get; set; } = true;
    public string? Warning { get; set; }
    public bool RequireConfirmation { get; set; }
    public object? Data { get; set; }
}

/// <summary>
/// 無資料的統一回應格式
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse OkEmpty() =>
        new() { Success = true };

    public static new ApiResponse Fail(string error, string? requestId = null) =>
        new() { Success = false, Error = error, RequestId = requestId };
}
