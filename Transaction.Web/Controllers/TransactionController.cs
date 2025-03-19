using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Transaction.Core.Entities;
using Transaction.Service.Services.Interfaces;

namespace Transaction.Web.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class TransactionController(ITransactionService transactionService, IFileService fileService)
        : ControllerBase
    {
        /// <summary>
        /// Imports transaction data from a csv file.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ImportData(IFormFile file, CancellationToken cancellationToken)
        {
            List<TransactionEntity> transactions;
            await using (var stream = file.OpenReadStream())
            {
                transactions = await fileService.ReadCsvFileAsync(stream, cancellationToken);
            }

            await transactionService.SaveToDbAsync(transactions, cancellationToken);

            return Ok();
        }
        
        /// <summary>
        /// Exports transactions within the specified date range to an Excel file.
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportTransactionsByDates([FromQuery]DateTime dateFrom, [FromQuery]DateTime dateTo, CancellationToken cancellationToken)
        {
            var startDateFrom = dateFrom.Date;
            var endDateTo = dateTo.Date.AddDays(1).AddTicks(-1);
        
            var transactions = await  transactionService.GetTransactionsByDatesAsync(startDateFrom, endDateTo, cancellationToken);
            var xlsx = await fileService.ConvertToExcelAsync(transactions, cancellationToken);
            return File(xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "transactions.xlsx");
        }
        
        /// <summary>
        /// Exports transactions within the specified date range and user's timezone to an Excel file.
        /// </summary>
        [HttpGet("export/users")]
        public async Task<IActionResult> ExportTransactionsByUserDates([FromQuery]DateTime dateFrom, [FromQuery]DateTime dateTo,[FromQuery]string timezone, CancellationToken cancellationToken)
        {
            var startDateFrom = dateFrom.Date;
            var endDateTo = dateTo.Date.AddDays(1).AddTicks(-1);
        
            var transactions = await  transactionService.GetTransactionsByUserDatesAsync(startDateFrom, endDateTo,timezone, cancellationToken);
            var xlsx = await fileService.ConvertToExcelAsync(transactions, cancellationToken);
            return File(xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "transactions.xlsx");
        }
        
        /// <summary>
        /// Exports transactions within january 2024 to an Excel file.
        /// </summary>
        [HttpGet("export/january")]
        public async Task<IActionResult> ExportJanuaryTransactions(CancellationToken cancellationToken)
        {
            var dateTimeFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var dateTimeTo = new DateTime(2024, 1, 31, 23, 59, 59, DateTimeKind.Utc);
        
            var transactions = await transactionService.GetTransactionsByDatesAsync(dateTimeFrom, dateTimeTo, cancellationToken);
            var xlsx = await fileService.ConvertToExcelAsync(transactions, cancellationToken);
            return File(xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "transactions.xlsx");
        }
    }
}