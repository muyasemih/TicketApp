namespace TicketApp.Services;

using TicketApp.DTOs;

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentRequestDto cardInfo);
    Task<PaymentResult> ProcessRefundAsync(string transactionId, decimal refundAmount);
}