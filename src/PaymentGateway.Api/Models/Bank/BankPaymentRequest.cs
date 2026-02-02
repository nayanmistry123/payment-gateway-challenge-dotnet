namespace PaymentGateway.Api.Models.Bank;

public class BankPaymentRequest
{
    private string CardNumber { get; set; }
    private string ExpiryDate { get; set; }
    private string Currency { get; set; }
    private int Amount { get; set; }
    private string Cvv { get; set; }

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