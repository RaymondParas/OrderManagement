using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderLibrary.Logger
{
    public class LogRecord
    {
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public string CustomMessage { get; set; }
        public DateTime LoggedAt { get; set; }
    }
}
