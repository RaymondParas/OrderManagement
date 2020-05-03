using Dapper;
using OrderLibrary.Logger;
using OrderLibrary.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace OrderLibrary.Helpers
{
    public class DBHelper : IDBHelper
    {
        private readonly ILogger _log;
        private const string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TheLetterDays;Integrated Security=True";
        private const string _orders = "[dbo].[Orders]";
        private Dictionary<string, Order> ExistingOrders = new Dictionary<string, Order>();

        public DBHelper(ILogger log)
        {
            _log = log;
        }

        public IEnumerable<Order> GetExistingOrders()
        {
            var query = $"SELECT * FROM  {_orders}";
            using (SqlConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Order>(query);
            }
        }

        public Order SelectQuery(string orderNumber)
        {
            var query = $"SELECT * FROM  {_orders} WHERE OrderNumber = '{orderNumber}'";

            using (SqlConnection db = new SqlConnection(_connectionString))
            {
                return db.Query<Order>(query).FirstOrDefault();
            }
        }

        public async Task<List<InsertedOrdersResponse>> InsertOrderQueryAsync(List<Order> orders)
        {
            var valuesClause = "@OrderNumber, @FirstName, @LastName, @Email, @PhoneNumber, @CompletedAt, " +
                               "@Status, @PaymentStatus, @TransactionId,@ShippingStatus, @ShippingAddress1, " +
                               "@ShippingAddress2, @ShippingCity, @ShippingState, @ShippingZip, @ShippingCountry, " +
                               "@Currency, @Items, @ItemCount, @ItemTotal, @TotalPrice, @TotalShipping, @TotalTax, " +
                               "@TotalDiscount, @DiscountCode, @Note, @PrivateNotes";
            var query = $"INSERT INTO {_orders} VALUES ({valuesClause})";

            List<InsertedOrdersResponse> insertedOrders = new List<InsertedOrdersResponse>();

            using (SqlConnection db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        foreach (var order in orders)
                        {
                            await db.ExecuteAsync(query, order, transaction);
                            insertedOrders.Add(CreateInsertedOrderResponse(order));
                        }
                        transaction.Commit();
                        return insertedOrders;
                    }
                    catch(Exception e)
                    {
                        await _log.LogException(e, $"Error when inserting orders");
                        transaction.Rollback();
                        db.Close();
                    }
                }
            }
            return null;
        }

        public async Task<bool> UpdateOrderQueryAsync(Order order, List<string> setClauses)
        {
            var setClause = string.Join(", ", setClauses.Select(c => c));
            var query = $"UPDATE {_orders} SET {setClause} WHERE OrderNumber = '{order.OrderNumber}'";

            using (SqlConnection db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        await db.ExecuteAsync(query, order, transaction);
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception e)
                    {
                        await _log.LogException(e, $"Error when updating order {order.OrderNumber}");
                        transaction.Rollback();
                        db.Close();
                    }
                }
            }
            return false;
        }

        public async Task<bool> DeleteQueryAsync(string orderNumber)
        {
            var query = $"DELETE FROM  {_orders} WHERE OrderNumber = '{orderNumber}'";

            using (SqlConnection db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        await db.ExecuteAsync(query, transaction: transaction);
                        transaction.Commit();
                        return true;
                    }
                    catch(Exception e)
                    {
                        await _log.LogException(e, $"Error when deleting order {orderNumber}");
                        transaction.Rollback();
                        db.Close();
                    }
                }
            }
            return false;
        }

        public async Task<bool> UpdateStatusAsync(string orderNumber, string newStatus, bool isOrderStatus)
        {
            var statusType = isOrderStatus ? "Status" : "PaymentStatus";
            var query = $"UPDATE {_orders} SET {statusType} = '{newStatus}' WHERE OrderNumber = '{orderNumber}'";

            using (SqlConnection db = new SqlConnection(_connectionString))
            {
                await db.OpenAsync();
                using (var transaction = db.BeginTransaction())
                {
                    try
                    {
                        await db.ExecuteAsync(query, transaction: transaction);
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception e)
                    {
                        await _log.LogException(e, $"Error updating status of order {orderNumber}");
                        transaction.Rollback();
                        db.Close();
                    }
                }
            }
            return false;
        }

        private InsertedOrdersResponse CreateInsertedOrderResponse(Order order)
        {
            return new InsertedOrdersResponse
            {
                OrderNumber = order.OrderNumber,
                TransactionId = order.TransactionId
            };
        }
    }
}
