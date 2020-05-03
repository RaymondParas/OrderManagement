using OrderLibrary.Helpers;
using OrderLibrary.Logger;
using OrderLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OrderLibrary.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDBHelper _db;
        private readonly ILogger _log;

        public OrderService(IDBHelper db, ILogger log)
        {
            _db = db;
            _log = log;
        }

        public IEnumerable<Order> GetExistingOrders()
        {
            return _db.GetExistingOrders();
        }

        public Order GetSpecificOrder(string orderNumber)
        {
            return _db.SelectQuery(orderNumber);
        }

        public List<object> GetOrderSummary()
        {
            var orders = GetExistingOrders();

            var orderSummary = new List<object>();
            foreach(var order in orders)
            {
                var item = CommonUtilities.MapStringToItems(order.Items);
                orderSummary.Add(new
                {
                    OrderNumber = order.OrderNumber,
                    ProductName = item.ProductName,
                    PricePerProduct = item.Price,
                    Quantity = item.Quantity,
                    TotalPrice = item.Total,
                    FirstName = order.FirstName,
                    LastName = order.LastName,
                    Email = order.Email,
                    Paid = order.PaymentStatus,
                    Status = order.Status
                });
            }
            return orderSummary;
        }

        public async Task<List<InsertedOrdersResponse>> InsertOrdersAsync(List<Order> orders)
        {
            var existingOrders = GetExistingOrders().ToDictionary(o => o.OrderNumber, o => o);
            orders.RemoveAll(o => existingOrders.ContainsKey(o.OrderNumber));

            return await _db.InsertOrderQueryAsync(orders);
        }

        public async Task<List<InsertedOrdersResponse>> InsertManualOrder(Order order)
        {
            order.OrderNumber = Guid.NewGuid().ToString();
            order.PhoneNumber = !string.IsNullOrWhiteSpace(order.PhoneNumber) ? order.PhoneNumber : string.Empty;
            order.CompletedAt = DateTime.Now;
            order.Status = OrderStatus.Pending.ToString();
            order.TransactionId = Guid.NewGuid().ToString();
            order.Currency = "CAD";
            order.Items = CommonUtilities.CreateItemsString(order.Items, order.ItemCount, order.ItemTotal);

            return await _db.InsertOrderQueryAsync(new List<Order>() { order });
        }

        public async Task<bool?> UpdateOrderAsync(string orderNumber, Order order)
        {
            var orderToUpdate = GetSpecificOrder(orderNumber);
            if (orderToUpdate == null)
                return null;

            var propertiesToChange = order.GetType().GetProperties().Where(p => p.PropertyType.IsValueType 
                                          ? !Activator.CreateInstance(p.PropertyType).Equals(p.GetValue(order, null))
                                          : null != p.GetValue(order, null));

            var setClauses = new List<string>();
            foreach (var property in propertiesToChange)
            {
                var prop = order.GetType().GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
                var value = prop.GetValue(order, null);

                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(orderToUpdate, value, null);
                    setClauses.Add($"{property.Name} = @{property.Name}");
                }
                else
                {
                    await _log.LogInfo($"Property '{property.Name}' is null or is not settable.");
                    return false;
                }
            }

            return await _db.UpdateOrderQueryAsync(orderToUpdate, setClauses);
        }

        public async Task<bool> DeleteOrderAsync(string orderNumber)
        {
            return await _db.DeleteQueryAsync(orderNumber);
        }

        public async Task<bool?> UpdateStatusAsync(string orderNumber, string newStatus, bool isOrderStatus)
        {
            var orderToUpdate = GetSpecificOrder(orderNumber);
            if (orderToUpdate == null)
                return null;

            string status = isOrderStatus ? CommonUtilities.GetOrderStatus(newStatus) 
                                          : CommonUtilities.GetPaymentStatus(newStatus);

            if (status == null)
                return null;

            return await _db.UpdateStatusAsync(orderNumber, status, isOrderStatus);
        }
    }
}
