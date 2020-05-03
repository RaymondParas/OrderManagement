using System;
using System.ComponentModel;

namespace OrderLibrary.Models
{
    public class Order
    {
        public string OrderNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CompletedAt { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionId { get; set; }
        public string ShippingStatus { get; set; }
        public string ShippingAddress1 { get; set; }
        public string ShippingAddress2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingZip { get; set; }
        public string ShippingCountry { get; set; }
        public string Currency { get; set; }
        public string Items { get; set; }
        public int ItemCount { get; set; }
        public decimal ItemTotal { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalShipping { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDiscount { get; set; }
        public string DiscountCode { get; set; }
        public string Note { get; set; }
        public string PrivateNotes { get; set; }
    }

    public class Items
    {
        public string ProductName { get; set; }
        public string ProductOptionName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }

    public enum OrderStatus
    {
        [Description("Pending")]
        Pending = 0,
        [Description("In Progress")]
        InProgress = 1,
        [Description("Completed")]
        Completed = 2,
        [Description("Shipped")]
        Shipped = 3
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Received = 1
    }

    public enum ShippingStatus
    {
        Unshipped = 0,
        Shipped = 1
    }
}
