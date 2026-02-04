namespace PaymentGateway.Api.Models.Internal;

/// <summary>
/// Internal class for a payment request
/// Contains sensitive information of Card Number and CVV
/// An object of this class should NEVER be persisted 
/// </summary>
public class MakePayment
{
    public MakePayment(
        Guid id,
        string cardNumber,
        DateTimeOffset expiryDate,
        string currency,
        int amount,
        string cvv)
    {
        Id = id;
        CardNumber = cardNumber;
        ExpiryDate = expiryDate;
        Currency = currency;
        Amount = amount;
        Cvv = cvv;
    }

    public Guid Id { get; set; }
    public string CardNumber { get; set; }
    public DateTimeOffset ExpiryDate { get; set; } 
    public string Currency { get; set; }
    public int Amount { get; set; }
    public string Cvv { get; set; }
}