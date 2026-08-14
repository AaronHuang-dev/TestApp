var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//app.UseHttpsRedirection();
app.UseStaticFiles();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/hello", () =>
{
    var forecast = "hello aaron";
    return forecast;
}).WithName("GetHello");




app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// 登入 API
app.MapPost("/login", (LoginRequest req) =>
{
    var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "users.csv");
    if (!File.Exists(csvPath))
        return Results.Problem("Cant find user data");
    
    var username = AuthService.Authenticate(
        File.ReadAllLines(csvPath),
        req.Username,
        req.Password);

    if (username is null)
        return Results.Json(new { success = false, message = "account/password incorrect"},
            statusCode: 401);
    
    return Results.Json(new { success = true, username});
});

// ── 新增會員 API ──
app.MapPost("/register", (RegisterRequest req) =>
{
    var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "users.csv");

    // 若檔案不存在，建立含標題列的新檔案
    if (!File.Exists(csvPath))
        File.WriteAllText(csvPath, "username,password\n");

    var csvLines = File.ReadAllLines(csvPath);

    // 檢查帳號是否已存在
    if (AuthService.UserExists(csvLines, req.Username))
        return Results.Json(new { success = false, message = "此帳號已被使用" },
            statusCode: 409);

    // 新增一行到 CSV
    File.AppendAllText(csvPath, $"{req.Username},{req.Password}\n");

    return Results.Json(new { success = true, message = $"帳號 {req.Username} 建立成功！" },
        statusCode: 201);
});

app.MapPut("/update", (UpdateRequest req) =>
{
    // check file (database)
    var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "users.csv");
    if (!File.Exists(csvPath))
        return Results.Problem("找不到使用者資料檔");
    
    var csvLines = File.ReadAllLines(csvPath);
    
    // validate current account
    var verified = AuthService.Authenticate(csvLines, req.Username, req.CurrentPassword);
    if (verified is null)
        return Results.Json(new { success = false, message = "帳號或目前密碼錯誤" },
            statusCode: 401);
    
    var updated_lines = AuthService.UpdatePassword(csvLines, req.Username, req.NewPassword);
    if (updated_lines is null)
    {
        return Results.Json(new { success = false, message = "找不到帳號" },
            statusCode: 401);
    }
    File.WriteAllLines(csvPath, updated_lines);
    return Results.Json(new { success=true, message="更新成功"}, statusCode: 201);
    
});

// ── 刪除會員 API ──
app.MapDelete("/delete", ([Microsoft.AspNetCore.Mvc.FromBody] DeleteRequest req) =>
{
    var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "users.csv");
    if (!File.Exists(csvPath))
        return Results.Problem("找不到使用者資料檔");

    if (string.IsNullOrWhiteSpace(req.Username))
        return Results.Json(new { success = false, message = "帳號不可為空" },
            statusCode: 400);

    var csvLines = File.ReadAllLines(csvPath);

    // 先驗證身分，確認是本人操作
    var verified = AuthService.Authenticate(csvLines, req.Username, req.Password);
    if (verified is null)
        return Results.Json(new { success = false, message = "帳號或密碼錯誤" },
            statusCode: 401);

    // 刪除使用者
    var updatedLines = AuthService.DeleteUser(csvLines, req.Username);
    if (updatedLines is null)
        return Results.Json(new { success = false, message = "刪除失敗，找不到帳號" },
            statusCode: 404);

    File.WriteAllLines(csvPath, updatedLines);

    return Results.Json(new { success = true, message = $"帳號 {req.Username} 已刪除！" });
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
record LoginRequest(string Username, string Password);
record RegisterRequest(string Username, string Password);

record UpdateRequest(string Username, string CurrentPassword, string NewPassword);

record DeleteRequest(string Username, string Password);