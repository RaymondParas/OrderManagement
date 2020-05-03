using OrderLibrary.Models;
using System.Collections.Generic;
using System.IO;

namespace OrderLibrary.Parsers
{
    public interface ICSVParser
    {
        List<Order> MapCSVToOrderModel(Stream filePath);
    }
}