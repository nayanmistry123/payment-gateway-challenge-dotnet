using Microsoft.AspNetCore.Mvc;

using Moq;

using NUnit.Framework;
using PaymentGateway.Api.Api;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Internal;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    
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
        var bankService = new BankService();
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
            .Returns(new BankPaymentResponse(true, "1234"));

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

    private PaymentsController InitialiseApp(IPaymentsRepository? paymentsRepository = null,
        IBankService? bankService = null)
    {
        var paymentsApi =
            new PaymentsApi(paymentsRepository ?? new PaymentsRepository(), bankService ?? new BankService());
        var paymentsController = new PaymentsController(paymentsApi);

        return paymentsController;
    }
}