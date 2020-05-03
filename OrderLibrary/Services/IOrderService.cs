using OrderLibrary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderLibrary.Services
{
    public interface IOrderService
    {
        IEnumerable<Order> GetExistingOrders();
        Order GetSpecificOrder(string orderNumber);
        List<object> GetOrderSummary();
        Task<List<InsertedOrdersResponse>> InsertOrdersAsync(List<Order> orders);
        Task<List<InsertedOrdersResponse>> InsertManualOrder(Order order);
        Task<bool?> UpdateOrderAsync(string orderNumber, Order order);
        Task<bool> DeleteOrderAsync(string orderNumber);
        Task<bool?> UpdateStatusAsync(string orderNumber, string newStatus, bool isOrderStatus);
    }
}