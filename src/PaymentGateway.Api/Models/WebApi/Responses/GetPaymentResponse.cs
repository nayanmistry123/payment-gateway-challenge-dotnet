namespace PaymentGateway.Api.Models.Responses;

public class GetPaymentResponse
{
    public GetPaymentResponse(
        Guid id,
        PaymentStatus status,
        string cardNumberLastFour,
        int expiryMonth,
        int expiryYear,
        string currency,
        int amount
        )
    {
        Id = id;
        Status = status;
        CardNumberLastFour = cardNumberLastFour;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Currency = currency;
        Amount = amount;
    }
    
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public string CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
}