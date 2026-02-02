namespace PaymentGateway.Api.Models.Bank;

public class BankPaymentRequest
{
    public string CardNumber { get; set; }
    public string ExpiryDate { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
    public string Cvv { get; set; }

    public BankPaymentRequest(
        string cardNumber,
        string expiryDate,
        string currency,
        int amount,
        string cvv
        )
    {
        CardNumber = cardNumber;
        ExpiryDate = expiryDate;
        Currency = currency;
        Amount = amount;
        Cvv = cvv;
    }
}