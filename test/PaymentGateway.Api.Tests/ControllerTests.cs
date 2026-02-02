using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using PaymentGateway.Api.Api;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Exceptions;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests;

public class ControllerTests
{
    [Theory]
    public async Task ProcessPayment_WhenApiThrowsUnknownErrorException_ReturnsExpectedResponse(bool isApiException)
    {
        var mockApi = new Mock<IPaymentsApi>();
        
        //We should handle any ApiException with a 500 code or any unrecognised exception as a 500
        if (isApiException)
            mockApi.Setup(p => p.MakePayment(It.IsAny<PostPaymentRequest>()))
                .Throws(() => new ApiException(ErrorSummary.InternalError, "Unexpected Error", 500));
        else 
            mockApi.Setup(p => p.MakePayment(It.IsAny<PostPaymentRequest>()))
                .Throws(() => new Exception("Some unexpected exception"));

        var controller = new PaymentsController(mockApi.Object);

        var response = await controller.ProcessPayment(GetExampleRequest());

        Assert.That(response.Result, Is.Not.Null);
        Assert.That(response.Result is ObjectResult);

        var objectResult = (ObjectResult)response.Result!;
        Assert.That(objectResult.StatusCode!, Is.EqualTo(500));
        
        Assert.That(objectResult.Value! is ErrorResponse);
        var errorResponse = (ErrorResponse)objectResult.Value!;
        Assert.That(errorResponse.ErrorSummary, Is.EqualTo(ErrorSummary.InternalError.ToString()));
        Assert.That(errorResponse.ErrorDetail, Is.EqualTo("An Unexpected Error Occurred"));
        Assert.That(errorResponse.StatusCode, Is.EqualTo(500));
    }

    [Theory]
    public async Task GetPayment_WhenApiThrowsUnknownErrorException_ReturnsExpectedResponse(bool isApiException)
    {
        var mockApi = new Mock<IPaymentsApi>();
        
        //We should handle any ApiException with a 500 code or any unrecognised exception as a 500
        if (isApiException)
            mockApi.Setup(p => p.GetPayment(It.IsAny<Guid>()))
                .Throws(() => new ApiException(ErrorSummary.InternalError, "Unexpected Error", 500));
        else 
            mockApi.Setup(p => p.GetPayment(It.IsAny<Guid>()))
                .Throws(() => new Exception("Some unexpected exception"));

        var controller = new PaymentsController(mockApi.Object);

        var response = await controller.GetPaymentAsync(Guid.NewGuid());

        Assert.That(response.Result, Is.Not.Null);
        Assert.That(response.Result is ObjectResult);

        var objectResult = (ObjectResult)response.Result!;
        Assert.That(objectResult.StatusCode!, Is.EqualTo(500));
        
        Assert.That(objectResult.Value! is ErrorResponse);
        var errorResponse = (ErrorResponse)objectResult.Value!;
        Assert.That(errorResponse.ErrorSummary, Is.EqualTo(ErrorSummary.InternalError.ToString()));
        Assert.That(errorResponse.ErrorDetail, Is.EqualTo("An Unexpected Error Occurred"));
        Assert.That(errorResponse.StatusCode, Is.EqualTo(500));
    }
    
    [Test]
    public async Task ProcessPayment_WhenApiReturnsExpectedPayment_ReturnsExpectedResponse()
    {
        var mockApi = new Mock<IPaymentsApi>();

        var exampleResponse = GetExampleResponse();
        mockApi
            .Setup(p => p.MakePayment(It.IsAny<PostPaymentRequest>()))
            .ReturnsAsync(exampleResponse);
        
        var controller = new PaymentsController(mockApi.Object);
        
        var response = await controller.ProcessPayment(GetExampleRequest());

        Assert.That(response.Result, Is.Not.Null);
        Assert.That(response.Result is OkObjectResult);

        var objectResult = (OkObjectResult)response.Result!;
        Assert.That(objectResult.StatusCode!, Is.EqualTo(200));
        
        Assert.That(objectResult.Value! is PaymentResponse);
        var paymentResponse = (PaymentResponse)objectResult.Value!;
       
        Assert.That(paymentResponse.Id, Is.EqualTo(exampleResponse.Id));
        Assert.That(paymentResponse.Status, Is.EqualTo(exampleResponse.Status));
        Assert.That(paymentResponse.Amount, Is.EqualTo(exampleResponse.Amount));
        Assert.That(paymentResponse.Currency, Is.EqualTo(exampleResponse.Currency));
        Assert.That(paymentResponse.ExpiryMonth, Is.EqualTo(exampleResponse.ExpiryMonth));
        Assert.That(paymentResponse.CardNumberLastFour, Is.EqualTo(exampleResponse.CardNumberLastFour));
    }
    
    [Test]
    public async Task GetPayment_WhenApiReturnsExpectedPayment_ReturnsExpectedResponse()
    {
        var mockApi = new Mock<IPaymentsApi>();

        var exampleResponse = GetExampleResponse();
        mockApi
            .Setup(p => p.GetPayment(It.Is<Guid>(g => g.Equals(exampleResponse.Id))))
            .Returns(exampleResponse);
        
        var controller = new PaymentsController(mockApi.Object);
        
        var response = await controller.GetPaymentAsync(exampleResponse.Id);

        Assert.That(response.Result, Is.Not.Null);
        Assert.That(response.Result is OkObjectResult);

        var objectResult = (OkObjectResult)response.Result!;
        Assert.That(objectResult.StatusCode!, Is.EqualTo(200));
        
        Assert.That(objectResult.Value! is PaymentResponse);
        var paymentResponse = (PaymentResponse)objectResult.Value!;
       
        Assert.That(paymentResponse.Id, Is.EqualTo(exampleResponse.Id));
        Assert.That(paymentResponse.Status, Is.EqualTo(exampleResponse.Status));
        Assert.That(paymentResponse.Amount, Is.EqualTo(exampleResponse.Amount));
        Assert.That(paymentResponse.Currency, Is.EqualTo(exampleResponse.Currency));
        Assert.That(paymentResponse.ExpiryMonth, Is.EqualTo(exampleResponse.ExpiryMonth));
        Assert.That(paymentResponse.CardNumberLastFour, Is.EqualTo(exampleResponse.CardNumberLastFour));
    }
    
    [Test]
    public async Task GetPayment_WhenApiReturnsNotFound_ReturnsExpectedResponse()
    {
        var mockApi = new Mock<IPaymentsApi>();

        var exampleResponse = GetExampleResponse();
        mockApi
            .Setup(p => p.GetPayment(It.Is<Guid>(g => g.Equals(exampleResponse.Id))))
            .Throws(() => new ApiException(
                ErrorSummary.PaymentNotFound,
                $"Payment with Id {exampleResponse.Id} does not exist",
                404));
        
        var controller = new PaymentsController(mockApi.Object);
        
        var response = await controller.GetPaymentAsync(exampleResponse.Id);

        Assert.That(response.Result, Is.Not.Null);
        Assert.That(response.Result is NotFoundObjectResult);

        var objectResult = (NotFoundObjectResult)response.Result!;

        Assert.That(objectResult.StatusCode!, Is.EqualTo(404));
        Assert.That(objectResult.Value! is ErrorResponse);
        var errorResponse = (ErrorResponse)objectResult.Value!;
        
        Assert.That(errorResponse.ErrorSummary, Is.EqualTo(ErrorSummary.PaymentNotFound.ToString()));
        Assert.That(errorResponse.ErrorDetail, Is.EqualTo($"Payment with Id {exampleResponse.Id} does not exist"));
        Assert.That(errorResponse.StatusCode, Is.EqualTo(404));
    }

    private PaymentResponse GetExampleResponse()
    {
        var futureTime = DateTimeOffset.Now.AddYears(1);
        return new PaymentResponse(
            Guid.NewGuid(),
            PaymentStatus.Authorized,
            "1234",
            futureTime.Month,
            futureTime.Year,
            "GBP",
            200
        );
    }
    
    private PostPaymentRequest GetExampleRequest()
    {
        var futureTime = DateTimeOffset.Now.AddYears(1);
        return new PostPaymentRequest(
            "123456789012345",
            futureTime.Month,
            futureTime.Year,
            "GBP",
            1200,
            "1234"
        );
    }
}