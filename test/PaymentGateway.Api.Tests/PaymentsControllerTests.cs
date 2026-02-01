using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using PaymentGateway.Api.Api;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Internal;
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
            PaymentStatus.Authorized
        );
        
        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);
        var paymentsApi = new PaymentsApi(paymentsRepository);
        var paymentsController = new PaymentsController(paymentsApi);

        var response = await paymentsController.GetPaymentAsync(payment.Id);
        Assert.That(response.Result is OkObjectResult);
        var result = ((OkObjectResult)response.Result).Value as GetPaymentResponse;
        
        Assert.That(result.Id, Is.EqualTo(payment.Id));
    }
    
    [Test]
    public async Task Returns404IfPaymentNotFound()
    {
        var controller = InitialiseApp();

        var response = controller.GetPaymentAsync(Guid.NewGuid());
        
        Assert.That(response.Result.Result is NotFoundObjectResult);
    }

    private PaymentsController InitialiseApp(PaymentsRepository? paymentsRepository = null)
    {
        var paymentsApi = new PaymentsApi(paymentsRepository ?? new PaymentsRepository());
        var paymentsController = new PaymentsController(paymentsApi);

        return paymentsController;
    }
    
}