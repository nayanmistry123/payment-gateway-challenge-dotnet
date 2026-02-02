using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.WebApi.Requests;

public class PostPaymentRequest
{
    /// <summary>
    /// The Card Number for the card making the payment
    /// </summary>
    [Required]
    public string CardNumber { get; }
    
    /// <summary>
    /// The Expiry Month for the card making the payment.
    /// Must be expressed in the format MM
    /// </summary>
    [Required]
    public int ExpiryMonth { get; }
    
    /// <summary>
    /// The Expiry Year for the card making the payment.
    /// Must be expressed in the format yyyy
    /// </summary>
    [Required]
    public int ExpiryYear { get; }
    
    /// <summary>
    /// The Currency of the payment.
    /// Must be one of GBP, USD or EUR.
    /// </summary>
    [Required]
    public string Currency { get; }
    
    /// <summary>
    /// The Amount of the Payment.
    /// Must be expressed in the minor unit of the currency.
    /// </summary>
    [Required]
    public int Amount { get; }
    
    /// <summary>
    /// The Cvv of the card making the payment
    /// </summary>
    [Required]
    public string Cvv { get; }

    public PostPaymentRequest(
        string cardNumber ,
        int expiryMonth,
        int expiryYear,
        string currency,
        int amount,
        string cvv
        )
    {
        CardNumber = cardNumber;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Currency = currency;
        Amount = amount;
        Cvv = cvv;
    }
}