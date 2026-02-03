using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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

using static PaymentGateway.Api.Tests.TestHelpers;

namespace PaymentGateway.Api.Tests.Integration;

public class PaymentsIntegrationTests
{
    [Test]
    public async Task RetrievesAnExistingPaymentSuccessfully()
    {
        //GIVEN an existing payment
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
        var paymentsController = InitialiseApp(paymentsRepository);
        
        //WHEN we retrieve that payment
        var response = await paymentsController.GetPaymentAsync(payment.Id);
        
        //THEN the payment is returned successfully
        Assert.That(response.Result is OkObjectResult);
        var result = ((OkObjectResult)response.Result!).Value as PaymentResponse;
        AssertEquals(result!, payment);
    }
    
    [Test]
    public async Task Returns404IfPaymentNotFound()
    {
        //GIVEN a payment that does not exist
        var controller = InitialiseApp();

        //WHEN we attempt to retrieve that payment
        var response = await controller.GetPaymentAsync(Guid.NewGuid());
        
        //THEN a 404 response is returned
        Assert.That(response.Result is NotFoundObjectResult);
    }

    [Test]
    public async Task CanCreatePayment_WithMockedBankService()
    {
        //GIVEN no payments exist
        var mockedBankService = new Mock<IBankService>();

        mockedBankService
            .Setup(b => b.MakePayment(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(new BankPaymentResponse(true, "1234"));

        var controller = InitialiseApp(bankService: mockedBankService.Object);

        //WHEN we create a payment
        var futureDate = DateTimeOffset.Now.AddYears(1);
        var paymentRequest = new PostPaymentRequest(
            "123456789012345",
            futureDate.Month,
            futureDate.Year,
            "GBP",
            1000,
            "1234"
        );

        //The payment is successful
        var response = await controller.ProcessPayment(paymentRequest);
        
        Assert.That(response.Result is OkObjectResult);
        var okResponse1 = (OkObjectResult)response.Result!;
        Assert.That(okResponse1.Value is PaymentResponse);
        var okPaymentResponse1 = (PaymentResponse)okResponse1.Value!;
        AssertEquals(paymentRequest, okPaymentResponse1);
        Assert.That(okPaymentResponse1.Status, Is.EqualTo("Authorized"));    
    }
    
    [Test]
    public async Task CanCreateAndRetrieveMultiplePayments_WithMockedBankService()
    {
        //GIVEN an instance of the application with a mocked bank service
        var mockedBankService = new Mock<IBankService>();
        mockedBankService
            .SetupSequence(b => b.MakePayment(It.IsAny<BankPaymentRequest>()))
            .ReturnsAsync(new BankPaymentResponse(true, "1234"))
            .ReturnsAsync(new BankPaymentResponse(false, "5678"));
        var controller = InitialiseApp(bankService: mockedBankService.Object);

        //WHEN we create a payment
        var futureDate = DateTimeOffset.Now.AddYears(1);
        var paymentRequest1 = new PostPaymentRequest(
            "123456789012345",
            futureDate.Month,
            futureDate.Year,
            "GBP",
            1000,
            "1234"
        );
        var request1Response = await controller.ProcessPayment(paymentRequest1);
        
        //THEN the payment is created correctly
        Assert.That(request1Response.Result is OkObjectResult);
        var okResponse1 = (OkObjectResult)request1Response.Result!;
        Assert.That(okResponse1.Value is PaymentResponse);
        var okPaymentResponse1 = (PaymentResponse)okResponse1.Value!;
        AssertEquals(paymentRequest1, okPaymentResponse1);
        Assert.That(okPaymentResponse1.Status, Is.EqualTo("Authorized"));
        
        //WHEN we create an additional payment
        var paymentRequest2 = new PostPaymentRequest(
            "098765432123456",
            futureDate.Month + 1,
            futureDate.Year + 2,
            "USD",
            2500,
            "0123"
        );
        var request2Response = await controller.ProcessPayment(paymentRequest2);
        
        //THEN the payment is created correctly
        Assert.That(request2Response.Result is OkObjectResult);
        var okResponse2 = (OkObjectResult)request2Response.Result!;
        Assert.That(okResponse2.Value is PaymentResponse);
        var okPaymentResponse2 = (PaymentResponse)okResponse2.Value!;
        AssertEquals(paymentRequest2, okPaymentResponse2);
        Assert.That(okPaymentResponse2.Status, Is.EqualTo("Declined"));
        
        //WHEN we request the first payment
        var retrievePayment1Response = await controller.GetPaymentAsync(okPaymentResponse1.Id);
        //THEN the payment is returned correctly 
        Assert.That(retrievePayment1Response.Result is OkObjectResult);
        var okRetrievePayment1Response = (OkObjectResult)retrievePayment1Response.Result!;
        Assert.That(okRetrievePayment1Response.Value is PaymentResponse);
        var okRetrievePayment1 = (PaymentResponse)okRetrievePayment1Response.Value!;
        AssertEquals(okRetrievePayment1, okPaymentResponse1);
        
        //WHEN we request the second payment
        var retrievePayment2Response = await controller.GetPaymentAsync(okPaymentResponse2.Id);
        //THEN the payment is returned correctly 
        Assert.That(retrievePayment2Response.Result is OkObjectResult);
        var okRetrievePayment2Response = (OkObjectResult)retrievePayment2Response.Result!;
        Assert.That(okRetrievePayment2Response.Value is PaymentResponse);
        var okRetrievePayment2 = (PaymentResponse)okRetrievePayment2Response.Value!;
        AssertEquals(okRetrievePayment2, okPaymentResponse2);
    }
    
    [Explicit("Requires a running bank service on localhost:8080")]
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
        Assert.That(paymentResponse.Status, Is.EqualTo(authorised ? PaymentStatus.Authorized : PaymentStatus.Declined));
        AssertEquals(paymentRequest, paymentResponse);

        var retrievedPaymentResponse = await controller.GetPaymentAsync(paymentResponse.Id);
        Assert.That(retrievedPaymentResponse.Result is OkObjectResult);
        var okRetrievedResult = (OkObjectResult)retrievedPaymentResponse.Result!;
        
        Assert.That(okRetrievedResult.Value is not null);
        var retrievedPayment = (PaymentResponse)okRetrievedResult.Value!;
        AssertEquals(retrievedPayment, paymentResponse);
    }


    private PaymentsController InitialiseApp(IPaymentsRepository? paymentsRepository = null,
        IBankService? bankService = null)
    {
        var paymentsApi =
            new PaymentsApi(
                paymentsRepository ?? new PaymentsRepository(), 
                bankService ?? new BankService(new HttpClient(), NullLogger<BankService>.Instance), 
                NullLogger<PaymentsApi>.Instance);
        var paymentsController = new PaymentsController(paymentsApi, NullLogger<PaymentsController>.Instance);

        return paymentsController;
   }
}