using System.ComponentModel.DataAnnotations;

namespace TicketApp.DTOs;

public class PaymentRequestDto
{
    [Required]
    public string CardHolderName { get; set; } = string.Empty;

    [Required]
    [CreditCard]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "Ay formatı 01-12 arasında olmalıdır.")]
    public string ExpireMonth { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{2}$", ErrorMessage = "Yıl formatı 2 haneli olmalıdır (örn: 26).")]
    public string ExpireYear { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV 3 haneli olmalıdır.")]
    public string Cvv { get; set; } = string.Empty;
}