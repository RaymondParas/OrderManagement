using OrderLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderLibrary.Helpers
{
    public interface IDBHelper
    {
        IEnumerable<Order> GetExistingOrders();
        Order SelectQuery(string orderNumber);
        Task<List<InsertedOrdersResponse>> InsertOrderQueryAsync(List<Order> orders);
        Task<bool> UpdateOrderQueryAsync(Order order, List<string> setClauses);
        Task<bool> DeleteQueryAsync(string orderNumber);
        Task<bool> UpdateStatusAsync(string orderNumber, string newStatus, bool isOrderStatus);
    }
}