/// <summary>
/// 帳號密碼驗證服務。
/// 將驗證邏輯從端點抽離，使其可被單元測試獨立測試。
/// </summary>
public static class AuthService
{
    /// <summary>
    /// 從 CSV 行清單中比對帳密。
    /// </summary>
    /// <param name="csvLines">CSV 全部行（含標題列）</param>
    /// <param name="username">待驗證的帳號</param>
    /// <param name="password">待驗證的密碼</param>
    /// <returns>驗證成功回傳使用者名稱；失敗回傳 null</returns>
    public static string? Authenticate(
        IEnumerable<string> csvLines,
        string username,
        string password)
    {
        var matched = csvLines
            .Skip(1)                              // 跳過標題列
            .Select(line => line.Split(','))
            .Where(parts => parts.Length == 2)    // 過濾格式錯誤的行
            .Select(parts => (
                Username: parts[0].Trim(),
                Password: parts[1].Trim()))
            .FirstOrDefault(u =>
                u.Username == username && u.Password == password);

        return matched == default ? null : matched.Username;
    }

   public static bool UserExists(IEnumerable<string> csvLines, string username)
    {
        return csvLines
            .Skip(1)
            .Select(line => line.Split(','))
            .Where(parts => parts.Length == 2)
            .Any(parts => parts[0].Trim().Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public static string[]? UpdatePassword(string[] csvLines, string username, string newPassword)
    {
        var lines = (string[])csvLines.Clone();   // 複製，不修改原始陣列
        for (int i = 1; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',');
            if (parts.Length == 2 && parts[0].Trim() == username)
            {
                lines[i] = $"{parts[0].Trim()},{newPassword}";
                return lines;
            }
        }
        return null;   // 帳號不存在
    }

    public static string[]? DeleteUser(string[] csvLines, string username)
    {
        var result = new List<string> { csvLines[0] };  // 保留標題列
        bool found = false;

        for (int i = 1; i < csvLines.Length; i++)
        {
            var parts = csvLines[i].Split(',');
            if (parts.Length == 2 && parts[0].Trim() == username)
                found = true;       // 略過這行（等於刪除）
            else
                result.Add(csvLines[i]);
        }

        return found ? result.ToArray() : null;
    }
}
