using System.Threading.Tasks;

namespace OrderLibrary.Logger
{
    public interface ILoggerDB
    {
        Task InsertLogAsync(LogRecord logRecord);
    }
}