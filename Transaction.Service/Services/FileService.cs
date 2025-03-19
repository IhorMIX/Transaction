using System.Globalization;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Transaction.Core.Entities;
using Transaction.Service.Services.Interfaces;

namespace Transaction.Service.Services;

public class FileService : IFileService
{
    /// <summary>
    /// We return data from csv file
    /// </summary>
    public async Task<List<TransactionEntity>> ReadCsvFileAsync(Stream file, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(file);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<TransactionCsvModel>();

        var records = new List<TransactionEntity>();

        await foreach (var record in csv.GetRecordsAsync<TransactionEntity>(cancellationToken))
        {
            records.Add(record);
        }

        return records;

    }
    
    /// <summary>
    /// We convert from csv to excel
    /// </summary>
    public async Task<byte[]> ConvertToExcelAsync(List<TransactionEntity> transactions, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Transactions");
        
        worksheet.Cell(1, 1).Value = "Transaction ID";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Email";
        worksheet.Cell(1, 4).Value = "Amount";
        worksheet.Cell(1, 5).Value = "Transaction Date";
        worksheet.Cell(1, 6).Value = "Client Location";
        worksheet.Cell(1, 6).Value = "TimeZone";
        
        for (int i = 0; i < transactions.Count; i++)
        {
            var row = i + 2;
            var transaction = transactions[i];

            worksheet.Cell(row, 1).Value = transaction.TransactionId;
            worksheet.Cell(row, 2).Value = transaction.Name;
            worksheet.Cell(row, 3).Value = transaction.Email;
            worksheet.Cell(row, 4).Value = transaction.Amount;
            worksheet.Cell(row, 5).Value = transaction.TransactionDate;
            worksheet.Cell(row, 6).Value = transaction.ClientLocation;
            worksheet.Cell(row, 6).Value = transaction.TimeZone;
        }
        
        worksheet.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return await Task.FromResult(stream.ToArray());
    }
}