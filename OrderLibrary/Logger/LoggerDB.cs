using Dapper;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace OrderLibrary.Logger
{
    public class LoggerDB : ILoggerDB
    {
        private const string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LogDB;Integrated Security=True";
        private const string _logTable = "[dbo].[Log]";
        public LoggerDB() { }

        public async Task InsertLogAsync(LogRecord logRecord)
        {
            var valuesClause = "@ExceptionType, @ExceptionMessage, @Source, @StackTrace, @CustomMessage, @LoggedAt";
            var query = $"INSERT INTO {_logTable} VALUES ({valuesClause})";

            using (SqlConnection db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();
                using (var t = db.BeginTransaction())
                {
                    try
                    {
                        await db.ExecuteAsync(query, logRecord, transaction: t);
                        t.Commit();
                    }
                    catch (Exception e)
                    {
                        WriteLogToFile(logRecord, e);
                        t.Rollback();
                        db.Close();
                    }
                }
            }
        }

        private void WriteLogToFile(LogRecord l, Exception e)
        {
            var date = DateTime.Today.ToString("yyyyMMdd");
            var basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            var directory = Path.Combine(basePath.Substring(6), "LogErrors");
            var filePath = Path.Combine(directory, $"Log_{date}.txt");

            var log = $"Error: {e.Message}\n\nAttempted Log: {l.CustomMessage}\n{l.ExceptionType}\t{l.Source}\t{l.LoggedAt}" +
                      $"\n{l.ExceptionMessage}\n{l.StackTrace}\n";

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine(log);
            }
        }
    }
}
