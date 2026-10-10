namespace TicketApp.Services;

using TicketApp.DTOs;

public class PaymentService : IPaymentService
{
    public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentRequestDto cardInfo)
    {
        // Ağ gecikmesi simülasyonu (200ms)
        await Task.Delay(200);

        var cleanCard = cardInfo.CardNumber.Replace(" ", "").Replace("-", "");

        // Simülasyon Kuralı: Kartın sonu 0000 ise kart reddedilsin
        if (cleanCard.EndsWith("0000"))
        {
            return new PaymentResult
            {
                IsSuccess = false,
                ErrorMessage = "Yetersiz bakiye veya kart limiti aşıldı."
            };
        }

        // Başarılı ödeme
        return new PaymentResult
        {
            IsSuccess = true,
            TransactionId = $"TXN-{Guid.NewGuid().ToString("N")[..12].ToUpper()}"
        };
    }

    public async Task<PaymentResult> ProcessRefundAsync(string transactionId, decimal refundAmount)
    {
        await Task.Delay(200);

        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return new PaymentResult
            {
                IsSuccess = false,
                ErrorMessage = "Geçersiz işlem numarası (TransactionId bulunamadı)."
            };
        }

        return new PaymentResult
        {
            IsSuccess = true,
            TransactionId = $"REF-{Guid.NewGuid().ToString("N")[..12].ToUpper()}"
        };
    }
}