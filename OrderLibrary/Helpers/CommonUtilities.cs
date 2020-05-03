using OrderLibrary.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace OrderLibrary.Helpers
{
    public static class CommonUtilities
    {
        public static Items MapStringToItems(string originalLine)
        {
            originalLine = originalLine + "|";
            var values = Regex.Matches(originalLine, @"[\:](.*?)(?=\|)");
            var listofValues = values.Cast<Match>().Select(match => match.Value.Replace(":", string.Empty)).ToList();

            return new Items
            {
                ProductName = listofValues[0],
                ProductOptionName = listofValues[1],
                Quantity = Convert.ToInt32(listofValues[2]),
                Price = Convert.ToDecimal(listofValues[3]),
                Total = Convert.ToDecimal(listofValues[4]),
            };
        }

        public static string CreateItemsString(string productName, int quantity, decimal price)
        {
            var total = quantity * price;
            var items = $"product_name:{productName}|product_option_name:|quantity:{quantity}|price:{price.ToString("0.0")}|total:{total.ToString("0.0")}";
            return items;
        }

        public static string GetDescriptionFromEnumValue(this Enum value)
        {
            return !(value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() is DescriptionAttribute attribute) ? value.ToString() : attribute.Description;
        }

        public static string GetOrderStatus(string status)
        {
            switch (status)
            {
                case "Pending":
                    return status;
                case "In Progress":
                    return status;
                case "Completed":
                    return status;
                case "Shipped":
                    return status;
                default:
                    return null;
            }
        }

        public static string GetPaymentStatus(string status)
        {
            switch (status)
            {
                case "Pending":
                    return status;
                case "Received":
                    return status;
                default:
                    return null;
            }
        }
    }
}
