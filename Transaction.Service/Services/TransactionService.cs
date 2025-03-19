using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using NodaTime;
using Npgsql;
using Transaction.Core.Entities;
using Transaction.Service.Options;
using Transaction.Service.Services.Interfaces;

namespace Transaction.Service.Services;

public class TransactionService(IOptions<DbConnectionString> connectionString, ILocationService locationService)
    : ITransactionService
{
    private readonly string _connectionString = connectionString.Value.ConnectionString;
    private readonly ILocationService _locationService = locationService;
    
    /// <summary>
    /// We return list of transactions from csv file
    /// </summary>
    public async Task<IEnumerable<TransactionEntity>> SaveToDbAsync(List<TransactionEntity> transactions, CancellationToken cancellationToken = default)
    {
        foreach (var transaction in transactions)
        {
            transaction.TimeZone =
                await _locationService.GetTimeZoneByLocation(transaction.ClientLocation, cancellationToken);
        }
        await using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    foreach (var record in transactions)
                    {
                        var query = @"
                            INSERT INTO ""Transactions"" (""TransactionId"", ""Name"", ""Email"", ""Amount"", ""TransactionDate"", ""ClientLocation"",""TimeZone"")
                            VALUES (@TransactionId, @Name, @Email, @Amount, @TransactionDate, @ClientLocation, @TimeZone)
                            ON CONFLICT (""TransactionId"") 
                            DO UPDATE SET
                                ""Name"" = EXCLUDED.""Name"",
                                ""Email"" = EXCLUDED.""Email"",
                                ""Amount"" = EXCLUDED.""Amount"",
                                ""TransactionDate"" = EXCLUDED.""TransactionDate"",
                                ""ClientLocation"" = EXCLUDED.""ClientLocation"",
                                ""TimeZone"" = EXCLUDED.""TimeZone"";";
                        
                        await connection.ExecuteAsync(query, record, transaction, commandType: CommandType.Text);
                    }

                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.CommitAsync(cancellationToken);
                    throw;
                }
            }
        }
        return transactions;
    }
    
    /// <summary>
    /// We return list of transactions for user who makes the request
    /// </summary>
    public async Task<List<TransactionEntity>> GetTransactionsByDatesAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
         var query = @"
            SELECT * FROM ""Transactions""
            WHERE ""TransactionDate"" BETWEEN @From AND @To
            ORDER BY ""TransactionDate"" DESC;";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var transactions = await connection.QueryAsync<TransactionEntity>(query, new { From = from, To = to });
        return transactions.ToList();
    }
    
    /// <summary>
    /// We return list of transactions by timezone which we chose
    /// </summary>
    public async Task<List<TransactionEntity>> GetTransactionsByUserDatesAsync(
        DateTime from, DateTime to, string userTimeZoneId, CancellationToken cancellationToken = default)
    {
        var userTimeZone = DateTimeZoneProviders.Tzdb[userTimeZoneId];

        var fromUtc = from.Kind == DateTimeKind.Utc ? from : 
            LocalDateTime.FromDateTime(from).InZoneLeniently(userTimeZone).ToDateTimeUtc();
        var toUtc = to.Kind == DateTimeKind.Utc ? to : 
            LocalDateTime.FromDateTime(to).InZoneLeniently(userTimeZone).ToDateTimeUtc();

        var query = @"
            SELECT * FROM ""Transactions""
            WHERE ""TransactionDate"" AT TIME ZONE @UserTimeZone AT TIME ZONE 'UTC' 
            BETWEEN @From AND @To
            ORDER BY ""TransactionDate"" DESC;";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var transactions = await connection.QueryAsync<TransactionEntity>(query, 
            new { From = fromUtc, To = toUtc, UserTimeZone = userTimeZoneId });

        return transactions.ToList();
    }


}