using NUnit.Framework;
using PaymentGateway.Api.Api;
using PaymentGateway.Api.Models.Exceptions;
using PaymentGateway.Api.Models.Requests;
using Assert = NUnit.Framework.Assert;

namespace PaymentGateway.Api.Tests;

[TestFixture]
public class ValidationTests
{
    static IEnumerable<TestCaseData> InvalidPaymentRequests()
    {
        yield return new TestCaseData(GetPaymentRequest(cardNumber: "1234567890123"),
            "Card Number must be 14-19 numeric characters").SetName("Short Card Number");
        yield return new TestCaseData(GetPaymentRequest(cardNumber: "12345678901234567890"),
            "Card Number must be 14-19 numeric characters").SetName("Long Card Number");
        yield return new TestCaseData(GetPaymentRequest(cardNumber: "1234567890123a"),
            "Card Number must be 14-19 numeric characters").SetName("Alphanumeric Card Number");
        
        yield return new TestCaseData(GetPaymentRequest(expiryMonth: 0),
            "Expiry Month must be between 1-12").SetName("0 Expiry Month");
        yield return new TestCaseData(GetPaymentRequest(expiryMonth: 13),
            "Expiry Month must be between 1-12").SetName("13 Expiry Month");
        
        yield return new TestCaseData(GetPaymentRequest(expiryMonth: DateTimeOffset.Now.AddMonths(-1).Month, expiryYear: DateTimeOffset.Now.Year),
            "Expiry Month and Year must be in the future").SetName("Current Year Previous Month");
        yield return new TestCaseData(GetPaymentRequest(expiryMonth: DateTimeOffset.Now.AddMonths(1).Month, expiryYear: DateTimeOffset.Now.AddYears(-11).Year),
            "Expiry Month and Year must be in the future").SetName("Previous Year Current Month");
        yield return new TestCaseData(GetPaymentRequest(expiryMonth: DateTimeOffset.Now.Month, expiryYear: DateTimeOffset.Now.Year),
            "Expiry Month and Year must be in the future").SetName("Current Year Current Month");
        
        yield return new TestCaseData(GetPaymentRequest(currency: "GBPP"),
            "Currency must be one of: USD, GBP, EUR").SetName("Invalid Length Currency Code");
        yield return new TestCaseData(GetPaymentRequest(currency: "AUD"),
            "Currency must be one of: USD, GBP, EUR").SetName("Unsupported Currency Code");
        
        yield return new TestCaseData(GetPaymentRequest(cvv: "12"),
            "Cvv must be 3-4 numeric characters").SetName("Short Cvv");
        yield return new TestCaseData(GetPaymentRequest(cvv: "12345"),
            "Cvv must be 3-4 numeric characters").SetName("Long Cvv");
        yield return new TestCaseData(GetPaymentRequest(cvv: "123a"),
            "Cvv must be 3-4 numeric characters").SetName("Alphanumeric Cvv");
    }
    
    [TestCaseSource(nameof(InvalidPaymentRequests))]
    public void GivenInvalidPaymentRequest_ThrowsException(
        PostPaymentRequest paymentRequest,
        string expectedFailure)
    {
        var exception = Assert.Throws<ApiException>(() => DtoValidator.ValidatePostPaymentRequest(paymentRequest));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.ErrorSummary, Is.EqualTo(ErrorSummary.InvalidPaymentRequest));
        Assert.That(exception.ErrorDetail, Does.Contain(expectedFailure));
    }

    
    #region  Helpers

    private static PostPaymentRequest GetPaymentRequest(
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

    #endregion
}