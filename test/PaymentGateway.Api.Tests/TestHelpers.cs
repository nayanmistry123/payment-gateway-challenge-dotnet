using NUnit.Framework;

using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Internal;
using PaymentGateway.Api.Models.Responses;
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
            paymentAuthorised,
            authorisationCode);
    }

    public static void AssertEquals(PostPaymentRequest paymentRequest, PaymentResponse paymentResponse)
    {
        Assert.That(paymentRequest.Amount, Is.EqualTo(paymentResponse.Amount));
        Assert.That(paymentRequest.Currency, Is.EqualTo(paymentResponse.Currency));
        Assert.That(paymentRequest.ExpiryMonth, Is.EqualTo(paymentResponse.ExpiryMonth));
        Assert.That(paymentRequest.ExpiryYear, Is.EqualTo(paymentResponse.ExpiryYear));
        Assert.That(paymentRequest.CardNumber.Substring(paymentRequest.CardNumber.Length-4), Is.EqualTo(paymentResponse.CardNumberLastFour));
    }
    
    public static void AssertEquals(PaymentResponse response1, PaymentResponse response2)
    {
        Assert.That(response1.Id, Is.EqualTo(response2.Id));
        Assert.That(response1.Amount, Is.EqualTo(response2.Amount));
        Assert.That(response1.Currency, Is.EqualTo(response2.Currency));
        Assert.That(response1.ExpiryMonth, Is.EqualTo(response2.ExpiryMonth));
        Assert.That(response1.ExpiryYear, Is.EqualTo(response2.ExpiryYear));
        Assert.That(response1.CardNumberLastFour, Is.EqualTo(response2.CardNumberLastFour));
        Assert.That(response1.Status, Is.EqualTo(response2.Status));
    }
    
    public static void AssertEquals(PaymentResponse response, Payment payment)
    {
        Assert.That(response.Id, Is.EqualTo(payment.Id));
        Assert.That(response.CardNumberLastFour, Is.EqualTo(payment.CardNumberLastFour));
        Assert.That(response.Currency, Is.EqualTo(payment.Currency));
        Assert.That(response.Amount, Is.EqualTo(payment.Amount));
        Assert.That(response.Status, Is.EqualTo(payment.PaymentAuthorised ? "Authorized" : "Declined"));
    }

}