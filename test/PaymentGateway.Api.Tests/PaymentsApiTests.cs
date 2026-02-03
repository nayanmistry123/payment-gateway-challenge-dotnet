using Moq;
using NUnit.Framework;
using PaymentGateway.Api.Api;
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
        //GIVEN an API
        var mockRepository = new Mock<IPaymentsRepository>();
        var mockBankService = new Mock<IBankService>();

        var guid = Guid.NewGuid();
        
        mockRepository
            .Setup(m => m.Get(It.Is<Guid>(g => g.Equals(guid))))
            .Returns((Payment?)null);

        //WHEN we request a payment that does not exist
        var api = new PaymentsApi(mockRepository.Object, mockBankService.Object);

        //THEN a 404 exception is returned
        var exception = Assert.Throws<ApiException>(() => api.GetPayment(guid));
        
        Assert.That(exception!.StatusCode, Is.EqualTo(404));
        Assert.That(exception!.ErrorDetail, Is.EqualTo($"Payment with Id {guid} does not exist"));
        Assert.That(exception!.ErrorSummary, Is.EqualTo(ErrorSummary.PaymentNotFound));
    }
    
    [Test]
    public void GetPayment_WhenPaymentDoesExist_ReturnsPayment()
    {
        //GIVEN an API 
        var mockRepository = new Mock<IPaymentsRepository>();
        var mockBankService = new Mock<IBankService>();

        var payment = GetExamplePayment();
        
        mockRepository
            .Setup(m => m.Get(It.Is<Guid>(g => g.Equals(payment.Id))))
            .Returns(payment);

        var api = new PaymentsApi(mockRepository.Object, mockBankService.Object);
        
        //WHEN we request an existing payment
        var response = api.GetPayment(payment.Id);
        
        //THEN the payment is returned
        AssertEquals(response, payment);
    }
    
    [Test]
    public async Task ProcessPayment_WithValidRequest_SavesPayment()
    {
        //GIVEN an API
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
        
        //WHEN we make a valid payment request
        var api = new PaymentsApi(mockRepository.Object, mockBankService.Object);
        var response = await api.MakePayment(paymentRequest);
        
        //THEN the response is successful
        AssertEquals(paymentRequest, response);
        
        //AND the payment has been saved 
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
    
    [Test]
    public async Task ProcessPayment_WithInvalidRequest_ThrowsApiException()
    {
        //GIVEN an api
        var mockRepository = new Mock<IPaymentsRepository>();
        var mockBankService = new Mock<IBankService>();
        var api = new PaymentsApi(mockRepository.Object, mockBankService.Object);

        //WHEN we make an invalid request
        var paymentRequest = GetPostPaymentRequest(cardNumber: "1");
        
        //THEN the API throws a 400 exception
        var exception = Assert.ThrowsAsync<ApiException>(() => api.MakePayment(paymentRequest));
        
        Assert.That(exception!.StatusCode, Is.EqualTo(400));
        Assert.That(exception!.ErrorSummary, Is.EqualTo(ErrorSummary.InvalidPaymentRequest));
        Assert.That(exception!.ErrorDetail, Is.EqualTo("Card Number must be 14-19 numeric characters"));
    }
}