using Dapper;
using Npgsql;
using StockPipeline.Shared.Models;

namespace StockPipeline.Processor;

public class DatabaseWriter
{
    private readonly string _connectionString;

    public DatabaseWriter(string connectionString) => _connectionString = connectionString;

    public async Task WriteQuoteAsync(StockQuote quote)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.ExecuteAsync(
            """
            INSERT INTO stock_quotes (symbol, price, open, high, low, volume, timestamp)
            VALUES (@Symbol, @Price, @Open, @High, @Low, @Volume, @Timestamp)
            """,
            quote);
    }

    public async Task WriteEnrichedAsync(EnrichedQuote eq)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.ExecuteAsync(
            """
            INSERT INTO moving_averages (symbol, timestamp, price, ma_5, ma_20, ma_50, price_change_pct)
            VALUES (@Symbol, @Timestamp, @Price, @Ma5, @Ma20, @Ma50, @PriceChangePct)
            """,
            eq);
    }
}
