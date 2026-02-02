using Moq;
using NUnit.Framework;
using PaymentGateway.Api.Api;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Exceptions;
using PaymentGateway.Api.Models.Internal;
using PaymentGateway.Api.Services;

using static PaymentGateway.Api.Tests.TestHelpers;

namespace PaymentGateway.Api.Tests;

public class PaymentsApiTests
{
    [Test]
    public void GetPayment_WhenPaymentDoesNotExist_ThrowsApiException()
    {
        var mockRepository = new Mock<IPaymentsRepository>();
        var mockBankService = new Mock<IBankService>();

        var guid = Guid.NewGuid();
        
        mockRepository
            .Setup(m => m.Get(It.Is<Guid>(g => g.Equals(guid))))
            .Returns((Payment?)null);

        var api = new PaymentsApi(mockRepository.Object, mockBankService.Object);

        var exception = Assert.Throws<ApiException>(() => api.GetPayment(guid));
        
        Assert.That(exception!.StatusCode, Is.EqualTo(404));
        Assert.That(exception!.ErrorDetail, Is.EqualTo($"Payment with Id {guid} does not exist"));
        Assert.That(exception!.ErrorSummary, Is.EqualTo(ErrorSummary.PaymentNotFound));
    }
    
    [Test]
    public void GetPayment_WhenPaymentDoesExist_ReturnsPayment()
    {
        var mockRepository = new Mock<IPaymentsRepository>();
        var mockBankService = new Mock<IBankService>();

        var payment = GetExamplePayment();
        
        mockRepository
            .Setup(m => m.Get(It.Is<Guid>(g => g.Equals(payment.Id))))
            .Returns(payment);

        var api = new PaymentsApi(mockRepository.Object, mockBankService.Object);
        var response = api.GetPayment(payment.Id);
        
        Assert.That(response.Id, Is.EqualTo(payment.Id));
        Assert.That(response.Amount, Is.EqualTo(payment.Amount));
        Assert.That(response.Currency, Is.EqualTo(payment.Currency));
        Assert.That(response.Status, Is.EqualTo(payment.PaymentAuthorised ? PaymentStatus.Authorized : PaymentStatus.Declined));
        Assert.That(response.ExpiryMonth, Is.EqualTo(payment.ExpiryDate.Month));
        Assert.That(response.ExpiryYear, Is.EqualTo(payment.ExpiryDate.Year));
        Assert.That(response.CardNumberLastFour, Is.EqualTo(payment.CardNumber.Substring(payment.CardNumber.Length-4)));
    }
    
    [Test]
    public async Task ProcessPayment_WithValidRequest_SavesPayment()
    {
        var mockRepository = new Mock<IPaymentsRepository>();
        var mockBankService = new Mock<IBankService>();

        var paymentRequest = GetPostPaymentRequest();
        
        var bankServiceResponse = new BankPaymentResponse(true, "12345678");
        
        mockBankService
            .Setup(m => m.MakePayment(It.Is<BankPaymentRequest>(
                req => req.CardNumber == paymentRequest.CardNumber
                       && req.Amount == paymentRequest.Amount
                       && req.Currency == paymentRequest.Currency
                       && req.Cvv == paymentRequest.Cvv)))
            .ReturnsAsync(bankServiceResponse);

        var savedPayments = new List<Payment>();
        mockRepository.Setup(m => m.Add(It.IsAny<Payment>())).Callback((Payment p) =>
        {
            savedPayments.Add(p);
        });
        
        var api = new PaymentsApi(mockRepository.Object, mockBankService.Object);
        var response = await api.MakePayment(paymentRequest);
        
        Assert.That(response.Amount, Is.EqualTo(paymentRequest.Amount));
        Assert.That(response.Currency, Is.EqualTo(paymentRequest.Currency));
        Assert.That(response.Status, Is.EqualTo(bankServiceResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined));
        Assert.That(response.ExpiryMonth, Is.EqualTo(paymentRequest.ExpiryMonth));
        Assert.That(response.ExpiryYear, Is.EqualTo(paymentRequest.ExpiryYear));
        Assert.That(response.CardNumberLastFour, Is.EqualTo(paymentRequest.CardNumber.Substring(paymentRequest.CardNumber.Length-4)));
        
        Assert.That(savedPayments.Count, Is.EqualTo(1));
        var savedPayment = savedPayments[0];
        Assert.That(savedPayment.Amount, Is.EqualTo(paymentRequest.Amount));
        Assert.That(savedPayment.Currency, Is.EqualTo(paymentRequest.Currency));
        Assert.That(savedPayment.ExpiryDate, Is.EqualTo(DateTimeOffset.Parse($"{paymentRequest.ExpiryYear}-{paymentRequest.ExpiryMonth}-01")));
        Assert.That(savedPayment.Cvv, Is.EqualTo(paymentRequest.Cvv));
        Assert.That(savedPayment.AuthorisationCode, Is.EqualTo(bankServiceResponse.AuthorizationCode));
        Assert.That(savedPayment.PaymentAuthorised, Is.EqualTo(bankServiceResponse.Authorized));
        Assert.That(savedPayment.CardNumber, Is.EqualTo(paymentRequest.CardNumber));
    }
}