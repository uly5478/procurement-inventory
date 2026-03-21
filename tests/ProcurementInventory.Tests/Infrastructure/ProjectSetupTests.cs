using FsCheck;
using FsCheck.Xunit;
using ProcurementInventory.Api.Models;
using Xunit;

namespace ProcurementInventory.Tests.Infrastructure;

/// <summary>
/// 驗證專案基礎架構設定正確
/// </summary>
public class ProjectSetupTests
{
    [Fact]
    public void ApiResponse_Ok_ShouldReturnSuccessTrue()
    {
        var response = ApiResponse<string>.Ok("test");

        Assert.True(response.Success);
        Assert.Equal("test", response.Data);
        Assert.Null(response.Error);
    }

    [Fact]
    public void ApiResponse_Fail_ShouldReturnSuccessFalse()
    {
        var response = ApiResponse<string>.Fail("發生錯誤", "req-123");

        Assert.False(response.Success);
        Assert.Equal("發生錯誤", response.Error);
        Assert.Equal("req-123", response.RequestId);
        Assert.Null(response.Data);
    }

    // Feature: procurement-inventory-management, Property 0: ApiResponse 統一格式不變式
    [Property]
    public Property ApiResponse_Ok_AlwaysHasSuccessTrue(string data)
    {
        var response = ApiResponse<string>.Ok(data);
        return response.Success.ToProperty();
    }

    // Feature: procurement-inventory-management, Property 0: ApiResponse Fail 統一格式不變式
    [Property]
    public Property ApiResponse_Fail_AlwaysHasSuccessFalse(string errorMessage)
    {
        var response = ApiResponse<string>.Fail(errorMessage ?? "error");
        return (!response.Success).ToProperty();
    }
}
