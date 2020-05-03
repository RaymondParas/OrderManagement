using OrderLibrary.Logger;
using OrderLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace OrderLibrary.Parsers
{
    public class CSVParser : ICSVParser
    {
        private readonly ILogger _log;
        private List<Order> _orders = new List<Order>();

        public CSVParser(ILogger log)
        {
            _log = log;
        }

        public List<Order> MapCSVToOrderModel(Stream stream)
        {
            using (var reader = new StreamReader(stream))
                while (!reader.EndOfStream)
                {
                    var fullLog = reader.ReadToEnd();
                    string[] allLines = fullLog.Split('\n');

                    for (int i = 1; i < allLines.Length; i++)
                    {
                        try
                        {
                            var values = Regex.Replace(allLines[i], @"[^A-Za-z0-9 \|\,\:\-\/\.\@_]", string.Empty, RegexOptions.Compiled).Split(',');
                            _orders.Add(new Order
                            {
                                OrderNumber = values[0],
                                FirstName = values[1],
                                LastName = values[2],
                                Email = values[3],
                                PhoneNumber = values[4],
                                CompletedAt = Convert.ToDateTime(values[5]),
                                Status = values[6].Equals(OrderStatus.Completed.ToString(), StringComparison.OrdinalIgnoreCase) 
                                         ? OrderStatus.Shipped.ToString() : OrderStatus.Pending.ToString(),
                                PaymentStatus = values[7].Equals(OrderStatus.Completed.ToString(), StringComparison.OrdinalIgnoreCase)
                                                ? PaymentStatus.Received.ToString() : PaymentStatus.Pending.ToString(),
                                TransactionId = values[8],
                                ShippingStatus = values[9].Equals(ShippingStatus.Shipped.ToString(), StringComparison.OrdinalIgnoreCase)
                                                ? ShippingStatus.Shipped.ToString() : ShippingStatus.Unshipped.ToString(),
                                ShippingAddress1 = values[10],
                                ShippingAddress2 = values[11],
                                ShippingCity = values[12],
                                ShippingState = values[13],
                                ShippingZip = values[14],
                                ShippingCountry = values[15],
                                Currency = values[16],
                                Items = values[17],
                                ItemCount = Convert.ToInt32(values[18]),
                                ItemTotal = Convert.ToDecimal(values[19]),
                                TotalPrice = Convert.ToDecimal(values[20]),
                                TotalShipping = Convert.ToDecimal(values[21]),
                                TotalTax = Convert.ToDecimal(values[22]),
                                TotalDiscount = Convert.ToDecimal(values[23]),
                                DiscountCode = values[24],
                                Note = values[25],
                                PrivateNotes = values[26]
                            });
                        }
                        catch (Exception e)
                        {
                            _log.LogException(e, "Error parsing CSV file");
                            return null;
                        }
                    }
                }
            return _orders;
        }
    }
}
