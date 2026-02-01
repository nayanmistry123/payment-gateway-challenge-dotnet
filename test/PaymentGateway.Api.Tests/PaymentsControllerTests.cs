using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

using PaymentGateway.Api.Api;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Internal;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    
    [Test]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new Payment(
            Guid.NewGuid(),
            "123456789012345",
            DateTimeOffset.Parse("2030-03-01"),
            "GBP",
            1250,
            "1234"
        );
        
        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var paymentsApi = new PaymentsApi(paymentsRepository);

        var paymentsController = new PaymentsController(paymentsApi);

        var response = await paymentsController.GetPaymentAsync(payment.Id);
        
        Assert.That(response.Result is OkObjectResult);
        Assert.That(response.Value!.Id, Is.EqualTo(payment.Id));
    }
    
    [Test]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
}