using Transaction.Core.Entities;

namespace Transaction.Service.Services.Interfaces;

public interface IFileService
{
    public Task<List<TransactionEntity>> ReadCsvFileAsync(Stream file, CancellationToken cancellationToken = default);
    public Task<byte[]> ConvertToExcelAsync (List<TransactionEntity> transactions, CancellationToken cancellationToken = default);
}