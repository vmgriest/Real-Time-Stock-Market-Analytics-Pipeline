using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace StockPipeline.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    private readonly string _connectionString;

    public StocksController(IConfiguration config) =>
        _connectionString = config.GetConnectionString("Postgres")!;

    [HttpGet("{symbol}/latest")]
    public async Task<IActionResult> GetLatest(string symbol)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        var result = await conn.QueryFirstOrDefaultAsync(
            """
            SELECT symbol, price, ma_5, ma_20, ma_50, price_change_pct, processed_at
            FROM   moving_averages
            WHERE  symbol = @symbol
            ORDER  BY processed_at DESC
            LIMIT  1
            """,
            new { symbol = symbol.ToUpperInvariant() });

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{symbol}/history")]
    public async Task<IActionResult> GetHistory(string symbol, [FromQuery] int hours = 24)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        var results = await conn.QueryAsync(
            """
            SELECT symbol, price, ma_5, ma_20, ma_50, price_change_pct, processed_at
            FROM   moving_averages
            WHERE  symbol = @symbol
              AND  processed_at >= NOW() - (@hours || ' hours')::INTERVAL
            ORDER  BY processed_at ASC
            """,
            new { symbol = symbol.ToUpperInvariant(), hours });

        return Ok(results);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        var results = await conn.QueryAsync(
            """
            SELECT DISTINCT ON (symbol)
                symbol,
                ROUND(price, 2)            AS price,
                ROUND(ma_5, 2)             AS ma5,
                ROUND(ma_20, 2)            AS ma20,
                ROUND(ma_50, 2)            AS ma50,
                ROUND(price_change_pct, 4) AS price_change_pct,
                processed_at
            FROM  moving_averages
            ORDER BY symbol, processed_at DESC
            """);

        return Ok(results);
    }
}
