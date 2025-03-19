using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Transaction.Core.Helpers;

namespace Transaction.Core.Entities;

public sealed class TransactionCsvModel : ClassMap<TransactionEntity>
{
    public TransactionCsvModel()
    {
        Map(m => m.TransactionId).Name("transaction_id");
        Map(m => m.Name).Name("name");
        Map(m => m.Email).Name("email");
        Map(m => m.Amount).Name("amount").TypeConverter<CustomConvertor>();;
        Map(m => m.TransactionDate).Name("transaction_date");
        Map(m => m.ClientLocation).Name("client_location");
    }
}
