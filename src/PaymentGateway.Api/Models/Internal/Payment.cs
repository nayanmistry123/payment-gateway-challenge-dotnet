namespace PaymentGateway.Api.Models.Internal;

/// <summary>
/// Internal class for a successfully stored payment
/// </summary>
public class Payment
{
    public Payment(
        Guid id,
        string cardNumberLastFour,
        DateTimeOffset expiryDate,
        string currency,
        int amount,
        bool paymentAuthorised,
        string authorisationCode)
    {
        Id = id;
        CardNumberLastFour = cardNumberLastFour;
        ExpiryDate = expiryDate;
        Currency = currency;
        Amount = amount;
        PaymentAuthorised = paymentAuthorised;
        AuthorisationCode = authorisationCode;
    }

    public Guid Id { get; set; }
    public string CardNumberLastFour { get; set; }
    public DateTimeOffset ExpiryDate { get; set; } 
    public string Currency { get; set; }
    public int Amount { get; set; }
    public bool PaymentAuthorised { get; set; }
    public string AuthorisationCode { get; set; }

}