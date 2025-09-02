using System.Text.Json.Serialization;

namespace ECommerceRealTimeApp.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RefundMethod
    {
        Original,   // Refund back to the original payment method
        PayPal,
        Stripe,
        BankTransfer,
        Manual
    }
}
