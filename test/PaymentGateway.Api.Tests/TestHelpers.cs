using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Internal;
using PaymentGateway.Api.Models.WebApi.Requests;

namespace PaymentGateway.Api.Tests;

public static class TestHelpers
{
    public static BankPaymentRequest GetExampleBankPaymentRequest(){
        var futureTime = DateTimeOffset.Now.AddYears(1);
        var bankPaymentRequest = new BankPaymentRequest(
            "123456789012345",
            futureTime.ToString("MM/yyyy"),
            "GBP",
            1000,
            "1234"
        );
        return bankPaymentRequest;
    }
    
    public static PostPaymentRequest GetPostPaymentRequest(
        string cardNumber = "12345678912345",
        int? expiryMonth = null,
        int? expiryYear = null,
        string currency = "GBP",
        int? amount = null,
        string cvv = "1234"
    )
    {
        return new PostPaymentRequest(
            cardNumber: cardNumber,
            expiryMonth: expiryMonth ?? 5,
            expiryYear: expiryYear ?? DateTimeOffset.Now.AddYears(1).Year,
            currency: currency,
            amount: amount ?? 5,
            cvv:  cvv
        );
    }    

    public static Payment GetExamplePayment(
        Guid? id = null,
        string cardNumber = "123456789012345",
        DateTimeOffset? expiryDate = null,
        string currency = "GBP",
        int amount = 1000,
        string cvv = "1234",
        bool paymentAuthorised = false,
        string authorisationCode = "1234"
    )
    {
        return new Payment(
            id ?? Guid.NewGuid(),
            cardNumber,
            expiryDate ?? DateTimeOffset.Parse("2020-01-01"),
            currency,
            amount,
            cvv,
            paymentAuthorised,
            authorisationCode);
    } 
    
}