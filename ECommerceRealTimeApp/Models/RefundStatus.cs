using System.Text.Json.Serialization;

namespace ECommerceRealTimeApp.Models
{
    // Enum to represent the status of a refund
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RefundStatus
    {
        Pending = 1,
        Completed = 6,
        Failed = 7
    }
}
