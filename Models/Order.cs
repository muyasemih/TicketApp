namespace TicketApp.Models;

public enum PaymentStatus
{
    Pending = 0,
    Paid = 1,
    PartiallyRefunded = 2,
    Refunded = 3,
    Failed = 4
}

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;

    public string? PaymentTransactionId { get; set; }

    public string? CardLastFourDigits { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}