using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Services;
using static PaymentGateway.Api.Tests.TestHelpers;

namespace PaymentGateway.Api.Tests;

public class BankServiceTests
{
    private readonly string _exampleJson = JsonSerializer.Serialize(new BankPaymentResponse(true, "1234"),
        new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

    private readonly HttpResponseMessage _fiveHundredError = new(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("", System.Text.Encoding.UTF8, "application/json")
        };
    
    [Test]
    public async Task MakePayment_GivenServiceReturns500FourTimes_ThrowsApiException()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(_fiveHundredError)
            .ReturnsAsync(_fiveHundredError)
            .ReturnsAsync(_fiveHundredError)            
            .ReturnsAsync(_fiveHundredError);
        
        var bankService = new BankService(GetHttpClient(handlerMock.Object));
        
        //Exception is caught at controller level 
        Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await bankService.MakePayment(GetExampleBankPaymentRequest());
        });        
        
        //Mock called 4 times - initial + 3 retries
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(4),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    
    [Test]
    public async Task MakePayment_GivenServiceReturns500ThreeTimes_RetriesThreeTimesSuccessfully()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        
        handlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(_fiveHundredError)
            .ReturnsAsync(_fiveHundredError)
            .ReturnsAsync(_fiveHundredError)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_exampleJson, System.Text.Encoding.UTF8, "application/json")
            });

        var bankService = new BankService(GetHttpClient(handlerMock.Object));
        
        var result = await bankService.MakePayment(GetExampleBankPaymentRequest());
        
        Assert.That(result.Authorized, Is.True);
        Assert.That(result.AuthorizationCode, Is.EqualTo("1234"));
        
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(4),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    
    [Test]
    public async Task MakePayment_GivenServiceReturns500Twice_RetriesTwiceSuccessfully()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        
        handlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(_fiveHundredError)
            .ReturnsAsync(_fiveHundredError)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_exampleJson, System.Text.Encoding.UTF8, "application/json")
            });

        var bankService = new BankService(GetHttpClient(handlerMock.Object));
        
        var result = await bankService.MakePayment(GetExampleBankPaymentRequest());
        
        Assert.That(result.Authorized, Is.True);
        Assert.That(result.AuthorizationCode, Is.EqualTo("1234"));
        
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(3),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    
    [Test]
    public async Task MakePayment_GivenServiceReturns500Once_RetriesOnceSuccessfully()
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        
        handlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(_fiveHundredError)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_exampleJson, System.Text.Encoding.UTF8, "application/json")
            });

        var bankService = new BankService(GetHttpClient(handlerMock.Object));
        
        var result = await bankService.MakePayment(GetExampleBankPaymentRequest());
        
        Assert.That(result.Authorized, Is.True);
        Assert.That(result.AuthorizationCode, Is.EqualTo("1234"));
        
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(2),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    
    [TestCase(true, "1234")]
    [TestCase(false, "123456789")]
    public async Task MakePayment_GivenServiceReturnsSuccess_ReturnsExpectedObject(
        bool authorised,
        string code)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        
        var responseJson = JsonSerializer.Serialize(new BankPaymentResponse(authorised, code),
            new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
        
        handlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            });

        var bankService = new BankService(GetHttpClient(handlerMock.Object));
        
        var result = await bankService.MakePayment(GetExampleBankPaymentRequest());
        
        Assert.That(result.Authorized, Is.EqualTo(authorised));
        Assert.That(result.AuthorizationCode, Is.EqualTo(code));
        
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
    
    private HttpClient GetHttpClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        { 
            BaseAddress = new Uri("http://localhost:8080/")
        };
    }
    
}