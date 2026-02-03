using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using PaymentGateway.Api.Api;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Exceptions;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.WebApi.Requests;
using PaymentGateway.Api.Models.WebApi.Responses;

using static PaymentGateway.Api.Tests.TestHelpers;

namespace PaymentGateway.Api.Tests;

public class ControllerTests
{
    [Theory]
    public async Task ProcessPayment_WhenApiThrowsUnknownErrorException_ReturnsExpectedResponse(bool isApiException)
    {
        //GIVEN a Mock API that throws an exception
        var mockApi = new Mock<IPaymentsApi>(MockBehavior.Strict);
        
        //We should handle any ApiException with a 500 code or any unrecognised exception as a 500
        if (isApiException)
            mockApi.Setup(p => p.MakePayment(It.IsAny<PostPaymentRequest>()))
                .Throws(() => new ApiException(ErrorSummary.InternalError, "Unexpected Error", 500));
        else 
            mockApi.Setup(p => p.MakePayment(It.IsAny<PostPaymentRequest>()))
                .Throws(() => new Exception("Some unexpected exception"));

        var controller = new PaymentsController(mockApi.Object);

        //WHEN we attempt a payment
        var response = await controller.ProcessPayment(GetPostPaymentRequest());

        Assert.That(response.Result, Is.Not.Null);
        Assert.That(response.Result is ObjectResult);

        //THEN a 500 error is returned
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
        //GIVEN a Mock API that throws an exception
        var mockApi = new Mock<IPaymentsApi>();
        
        //We should handle any ApiException with a 500 code or any unrecognised exception as a 500
        if (isApiException)
            mockApi.Setup(p => p.GetPayment(It.IsAny<Guid>()))
                .Throws(() => new ApiException(ErrorSummary.InternalError, "Unexpected Error", 500));
        else 
            mockApi.Setup(p => p.GetPayment(It.IsAny<Guid>()))
                .Throws(() => new Exception("Some unexpected exception"));

        var controller = new PaymentsController(mockApi.Object);

        //WHEN we attempt to retrieve a payment
        var response = await controller.GetPaymentAsync(Guid.NewGuid());

        Assert.That(response.Result, Is.Not.Null);
        Assert.That(response.Result is ObjectResult);

        //THEN a 500 error is returned
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
        //GIVEN an api that returns a valid payment response
        var mockApi = new Mock<IPaymentsApi>();

        var exampleResponse = GetExampleResponse();
        mockApi
            .Setup(p => p.MakePayment(It.IsAny<PostPaymentRequest>()))
            .ReturnsAsync(exampleResponse);
        
        var controller = new PaymentsController(mockApi.Object);
        
        //WHEN we make a payment
        var response = await controller.ProcessPayment(GetPostPaymentRequest());

        Assert.That(response.Result, Is.Not.Null);
        Assert.That(response.Result is OkObjectResult);

        //THEN the response is successful
        var objectResult = (OkObjectResult)response.Result!;
        Assert.That(objectResult.StatusCode!, Is.EqualTo(200));
        
        Assert.That(objectResult.Value! is PaymentResponse);
        var paymentResponse = (PaymentResponse)objectResult.Value!;
       
        AssertEquals(paymentResponse, exampleResponse);
    }
    
    [Test]
    public async Task GetPayment_WhenApiReturnsExpectedPayment_ReturnsExpectedResponse()
    {
        //GIVEN an api that returns a valid payment response
        var mockApi = new Mock<IPaymentsApi>();

        var exampleResponse = GetExampleResponse();
        mockApi
            .Setup(p => p.GetPayment(It.Is<Guid>(g => g.Equals(exampleResponse.Id))))
            .Returns(exampleResponse);
        
        var controller = new PaymentsController(mockApi.Object);
        
        //WHEN we attempt to retrieve a payment
        var response = await controller.GetPaymentAsync(exampleResponse.Id);

        Assert.That(response.Result, Is.Not.Null);
        Assert.That(response.Result is OkObjectResult);

        //THEN the response is successful
        var objectResult = (OkObjectResult)response.Result!;
        Assert.That(objectResult.StatusCode!, Is.EqualTo(200));
        
        Assert.That(objectResult.Value! is PaymentResponse);
        var paymentResponse = (PaymentResponse)objectResult.Value!;
       
        AssertEquals(paymentResponse, exampleResponse);
    }
    
    [Test]
    public async Task GetPayment_WhenApiReturnsNotFound_ReturnsExpectedResponse()
    {
        //GIVEN an Api that throws a 404 exception
        var mockApi = new Mock<IPaymentsApi>();

        var exampleResponse = GetExampleResponse();
        mockApi
            .Setup(p => p.GetPayment(It.Is<Guid>(g => g.Equals(exampleResponse.Id))))
            .Throws(() => new ApiException(
                ErrorSummary.PaymentNotFound,
                $"Payment with Id {exampleResponse.Id} does not exist",
                404));
        
        var controller = new PaymentsController(mockApi.Object);
        
        //WHEN we attempt to retrieve a payment 
        var response = await controller.GetPaymentAsync(exampleResponse.Id);

        Assert.That(response.Result, Is.Not.Null);
        Assert.That(response.Result is NotFoundObjectResult);

        //THEN a 404 response is returned
        var objectResult = (NotFoundObjectResult)response.Result!;

        Assert.That(objectResult.StatusCode!, Is.EqualTo(404));
        Assert.That(objectResult.Value! is ErrorResponse);
        var errorResponse = (ErrorResponse)objectResult.Value!;
        
        Assert.That(errorResponse.ErrorSummary, Is.EqualTo(ErrorSummary.PaymentNotFound.ToString()));
        Assert.That(errorResponse.ErrorDetail, Is.EqualTo($"Payment with Id {exampleResponse.Id} does not exist"));
        Assert.That(errorResponse.StatusCode, Is.EqualTo(404));
    }

    [Theory]
    public async Task ProcessPayment_WhenApiThrowsValidationError_ReturnsExpectedResponse(bool isApiException)
    {
        //GIVEN a Mock API that throws a 400 validation exception
        var mockApi = new Mock<IPaymentsApi>(MockBehavior.Strict);
        
        mockApi.Setup(p => p.MakePayment(It.IsAny<PostPaymentRequest>()))
                .Throws(() => new ApiException(ErrorSummary.InvalidPaymentRequest, "The request was invalid", 400));

        var controller = new PaymentsController(mockApi.Object);

        //WHEN we attempt a payment
        var response = await controller.ProcessPayment(GetPostPaymentRequest());

        Assert.That(response.Result, Is.Not.Null);
        Assert.That(response.Result is ObjectResult);

        //THEN a 400 error is returned
        var objectResult = (BadRequestObjectResult)response.Result!;
        Assert.That(objectResult.StatusCode!, Is.EqualTo(400));
        
        Assert.That(objectResult.Value! is ErrorResponse);
        var errorResponse = (ErrorResponse)objectResult.Value!;
        Assert.That(errorResponse.ErrorSummary, Is.EqualTo(ErrorSummary.InvalidPaymentRequest.ToString()));
        Assert.That(errorResponse.ErrorDetail, Is.EqualTo("The request was invalid"));
        Assert.That(errorResponse.StatusCode, Is.EqualTo(400));
    }
    
    private PaymentResponse GetExampleResponse()
    {
        var futureTime = DateTimeOffset.Now.AddYears(1);
        return new PaymentResponse(
            Guid.NewGuid(),
            PaymentStatus.Authorized.ToString(),
            "1234",
            futureTime.Month,
            futureTime.Year,
            "GBP",
            200
        );
    }
}