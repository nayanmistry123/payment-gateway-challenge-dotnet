namespace PaymentGateway.Api.Models.Responses;

public class PaymentResponse
{
    /// <summary>
    /// The unique identifier for this payment
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// The status of this payment. Authorized or Declined
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// The last four digits of the Card Number for this payment
    /// </summary>
    public string CardNumberLastFour { get; set; }
    
    /// <summary>
    /// The Expiry Month of the Card for this payment
    /// </summary>
    public int ExpiryMonth { get; set; }
    
    /// <summary>
    /// The Expiry Year of the Card for this payment
    /// </summary>
    public int ExpiryYear { get; set; }
    
    /// <summary>
    /// The Currency of this payment
    /// </summary>
    public string Currency { get; set; }
    
    /// <summary>
    /// The Amount of this payment, in the minor currency unit
    /// </summary>
    public int Amount { get; set; }

    public PaymentResponse(
        Guid id,
        string status,
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
}
