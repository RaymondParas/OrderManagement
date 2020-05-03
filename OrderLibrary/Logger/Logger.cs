using System;
using System.Threading.Tasks;

namespace OrderLibrary.Logger
{
    public class Logger : ILogger
    {
        private readonly ILoggerDB _db;
        public Logger(ILoggerDB db)
        {
            _db = db;
        }

        public async Task LogException(Exception e, string message = null)
        {
            var logRecord = new LogRecord
            {
                ExceptionType = e.GetType().FullName,
                ExceptionMessage = e.Message,
                Source = e.Source,
                StackTrace = e.StackTrace,
                CustomMessage = message,
                LoggedAt = DateTime.Now
            };

            await _db.InsertLogAsync(logRecord);
        }

        public async Task LogInfo(string message)
        {
            var logRecord = new LogRecord
            {
                ExceptionType = "N/A",
                ExceptionMessage = "Information Log",
                Source = "N/A",
                StackTrace = "N/A",
                CustomMessage = message,
                LoggedAt = DateTime.Now
            };

            await _db.InsertLogAsync(logRecord);
        }
    }
}
