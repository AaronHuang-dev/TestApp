using Xunit;

namespace TestApp.Tests;


public class AuthServiceTests
{
    // 測試用的假 CSV 資料（不依賴真實檔案）
    private readonly string[] _csvLines =
    [
        "username,password",
        "admin,admin123",
        "user1,password1"
    ];

    // ── [Fact] 單一固定案例 ──────────────────────────────

    [Fact]
    public void Authenticate_正確帳密_回傳使用者名稱()
    {
        // Arrange（準備）
        var username = "admin";
        var password = "admin123";

        // Act（執行）
        var result = AuthService.Authenticate(_csvLines, username, password);

        // Assert（驗證）
        Assert.Equal("admin", result);
    }

    [Fact]
    public void Authenticate_第二位使用者正確帳密_回傳使用者名稱()
    {
        var result = AuthService.Authenticate(_csvLines, "user1", "password1");
        Assert.Equal("user1", result);
    }

    [Fact]
    public void Authenticate_CSV有多餘空格_仍可比對成功()
    {
        // 驗證 .Trim() 是否正確處理前後空白
        var csvWithSpaces = new[]
        {
            "username,password",
            " admin , admin123 "
        };

        var result = AuthService.Authenticate(csvWithSpaces, "admin", "admin123");

        Assert.Equal("admin", result);
    }

    [Fact]
    public void Authenticate_CSV只有標題列_回傳null()
    {
        // 驗證 .Skip(1) 後清單為空時不崩潰
        var headerOnly = new[] { "username,password" };

        var result = AuthService.Authenticate(headerOnly, "admin", "admin123");

        Assert.Null(result);
    }

    // ── [Theory] 多組資料一起測 ─────────────────────────

    [Theory]
    [InlineData("admin",  "wrongpass")]   // 密碼錯誤
    [InlineData("nobody", "password1")]   // 帳號不存在
    [InlineData("",       "")]            // 空白輸入
    [InlineData("ADMIN",  "admin123")]    // 大小寫不符（區分大小寫）
    public void Authenticate_無效輸入_皆回傳null(string username, string password)
    {
        var result = AuthService.Authenticate(_csvLines, username, password);
        Assert.Null(result);
    }

    // ── UserExists 測試 ──────────────────────────────────

    [Fact]
    public void UserExists_已存在的帳號_回傳true()
    {
        Assert.True(AuthService.UserExists(_csvLines, "admin"));
    }

    [Fact]
    public void UserExists_不存在的帳號_回傳false()
    {
        Assert.False(AuthService.UserExists(_csvLines, "nobody"));
    }

    [Fact]
    public void UserExists_大小寫不同視為同帳號_回傳true()
    {
        // UserExists 用 OrdinalIgnoreCase，避免大小寫重複帳號
        Assert.True(AuthService.UserExists(_csvLines, "ADMIN"));
    }

    [Fact]
    public void UserExists_空白帳號_回傳false()
    {
        Assert.False(AuthService.UserExists(_csvLines, ""));
    }

    // ── UpdatePassword 測試 ──────────────────────────────

    [Fact]
    public void UpdatePassword_正確帳號_回傳含新密碼的陣列()
    {
        // Arrange
        var newPassword = "newpassword";

        // Act
        var result = AuthService.UpdatePassword(_csvLines, "admin", newPassword);

        // Assert：回傳值不為 null，且包含更新後的行
        Assert.NotNull(result);
        Assert.Contains("admin,newpassword", result);
    }

    [Fact]
    public void UpdatePassword_不存在的帳號_回傳null()
    {
        var result = AuthService.UpdatePassword(_csvLines, "nobody", "newpassword");
        Assert.Null(result);
    }

    [Fact]
    public void UpdatePassword_更新後行數不變()
    {
        // 只改密碼，不應該增加或減少行數
        var result = AuthService.UpdatePassword(_csvLines, "admin", "newpassword");

        Assert.NotNull(result);
        Assert.Equal(_csvLines.Length, result!.Length);
    }

    [Fact]
    public void UpdatePassword_不影響原始陣列()
    {
        // 驗證 Clone() 的效果：原始陣列不應被修改
        var originalLine = _csvLines[1];   // "admin,admin123"
        AuthService.UpdatePassword(_csvLines, "admin", "newpassword");
        Assert.Equal(originalLine, _csvLines[1]);
    }

    // ── DeleteUser 測試 ──────────────────────────────────

    [Fact]
    public void DeleteUser_正確帳號_回傳不含該帳號的陣列()
    {
        // Arrange & Act
        var result = AuthService.DeleteUser(_csvLines, "admin");

        // Assert：回傳值不為 null，且刪除的帳號不再出現
        Assert.NotNull(result);
        Assert.DoesNotContain(result, line => line.StartsWith("admin,"));
    }

    [Fact]
    public void DeleteUser_不存在的帳號_回傳null()
    {
        var result = AuthService.DeleteUser(_csvLines, "nobody");
        Assert.Null(result);
    }

    [Fact]
    public void DeleteUser_刪除後行數減一()
    {
        // 刪除一筆，行數應從 3（標題+2筆）變成 2
        var result = AuthService.DeleteUser(_csvLines, "admin");

        Assert.NotNull(result);
        Assert.Equal(_csvLines.Length - 1, result!.Length);
    }

    [Fact]
    public void DeleteUser_不影響原始陣列()
    {
        // 因為 DeleteUser 建立新 List 而非修改傳入陣列，原始行數不應改變
        var originalLength = _csvLines.Length;
        AuthService.DeleteUser(_csvLines, "admin");
        Assert.Equal(originalLength, _csvLines.Length);
    }
}
