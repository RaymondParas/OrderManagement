using System;
using System.Threading.Tasks;

namespace OrderLibrary.Logger
{
    public interface ILogger
    {
        Task LogException(Exception e, string message = null);
        Task LogInfo(string message);
    }
}