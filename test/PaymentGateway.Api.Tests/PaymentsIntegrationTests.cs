using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using PaymentGateway.Api.Api;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Internal;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.WebApi.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsIntegrationTests
{
    [Test]
    public async Task RetrievesAnExistingPaymentSuccessfully()
    {
        // Arrange
        var payment = new Payment(
            Guid.NewGuid(),
            "123456789012345",
            DateTimeOffset.Parse("2030-03-01"),
            "GBP",
            1250,
            "1234",
            true,
            "1234"
        );
        
        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);
        var bankService = new BankService(new HttpClient());
        var paymentsApi = new PaymentsApi(paymentsRepository, bankService);
        var paymentsController = new PaymentsController(paymentsApi);

        var response = await paymentsController.GetPaymentAsync(payment.Id);
        Assert.That(response.Result is OkObjectResult);
        var result = ((OkObjectResult)response.Result).Value as PaymentResponse;
        
        Assert.That(result.Id, Is.EqualTo(payment.Id));
    }
    
    [Test]
    public async Task Returns404IfPaymentNotFound()
    {
        var controller = InitialiseApp();

        var response = await controller.GetPaymentAsync(Guid.NewGuid());
        
        Assert.That(response.Result is NotFoundObjectResult);
    }

    [Test]
    public async Task CanCreateAndRetrieveAPayment_WithMockedBankService()
    {
        var mockedBankService = new Mock<IBankService>();

        mockedBankService
            .Setup(b => b.MakePayment(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(new BankPaymentResponse(true, "1234"));

        var controller = InitialiseApp(bankService: mockedBankService.Object);

        var futureDate = DateTimeOffset.Now.AddYears(1);
        var paymentRequest = new PostPaymentRequest(
            "123456789012345",
            futureDate.Month,
            futureDate.Year,
            "GBP",
            1000,
            "1234"
        );

        var response = await controller.ProcessPayment(paymentRequest);
        
        Assert.That(response.Result is OkObjectResult o);
    }

    // Requires a running bank service on localhost:8080
    [Theory]
    public async Task CanCreateAndRetrieveAPayment_WithRealBankService(bool authorised)
    {
        //GIVEN an instance of the API connected to a localhost bank simulator
        var controller = InitialiseApp();

        var futureDate = DateTimeOffset.Now.AddYears(1);
        var paymentRequest = new PostPaymentRequest(
            authorised ? "123456789012345" : "123456789012346",
            futureDate.Month,
            futureDate.Year,
            "GBP",
            1000,
            "1234"
        );
        //WHEN we post a valid payment request
        var response = await controller.ProcessPayment(paymentRequest);

        //THEN the response is successful
        Assert.That(response.Result is OkObjectResult);
        var okResponseResult = (OkObjectResult)response.Result!;

        Assert.That(okResponseResult.Value is not null);
        var paymentResponse = (PaymentResponse)okResponseResult.Value!;
        
        //AND the response contains the expected payment information
        Assert.That(paymentResponse.Id, Is.Not.Null);
        Assert.That(paymentResponse.Status, Is.EqualTo(authorised ? PaymentStatus.Authorized : PaymentStatus.Declined));
        Assert.That(paymentResponse.Amount, Is.EqualTo(1000));
        Assert.That(paymentResponse.ExpiryMonth, Is.EqualTo(futureDate.Month));
        Assert.That(paymentResponse.ExpiryYear, Is.EqualTo(futureDate.Year));
        Assert.That(paymentResponse.CardNumberLastFour, Is.EqualTo(authorised ? "2345" : "2346"));

        var retrievedPaymentResponse = await controller.GetPaymentAsync(paymentResponse.Id);
        Assert.That(retrievedPaymentResponse.Result is OkObjectResult);
        var okRetrievedResult = (OkObjectResult)retrievedPaymentResponse.Result!;
        
        Assert.That(okRetrievedResult.Value is not null);
        var retrievedPayment = (PaymentResponse)okRetrievedResult.Value!;
        
        Assert.That(retrievedPayment.Id, Is.EqualTo(paymentResponse.Id));
        Assert.That(retrievedPayment.Status, Is.EqualTo(authorised ? PaymentStatus.Authorized : PaymentStatus.Declined));
        Assert.That(retrievedPayment.Amount, Is.EqualTo(1000));
        Assert.That(retrievedPayment.ExpiryMonth, Is.EqualTo(futureDate.Month));
        Assert.That(retrievedPayment.ExpiryYear, Is.EqualTo(futureDate.Year));
        Assert.That(retrievedPayment.CardNumberLastFour, Is.EqualTo(authorised ? "2345" : "2346"));
    }


    private PaymentsController InitialiseApp(IPaymentsRepository? paymentsRepository = null,
        IBankService? bankService = null)
    {
        var paymentsApi =
            new PaymentsApi(paymentsRepository ?? new PaymentsRepository(), bankService ?? new BankService(new HttpClient()));
        var paymentsController = new PaymentsController(paymentsApi);

        return paymentsController;
    }
}