using System.Text.Json;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Exceptions;

using Polly;
using Polly.Retry;

namespace PaymentGateway.Api.Services;

public class BankService : IBankService
{
    private readonly HttpClient _client;
    private readonly ILogger<BankService> _logger;
    
    //Ensure we serialise objects with snake case to match the BankService expected JSON format
    private JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public BankService(HttpClient httpClient, ILogger<BankService> logger)
    {
        _client = httpClient;
        _logger = logger;
    }
    
    //Retry on any 500 errors, as they may be transient
    private  AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy(ILogger<BankService> logger) =>
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => ((int)msg.StatusCode >= 500 && (int)msg.StatusCode <= 599)) 
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var statusCode = outcome.Result?.StatusCode;
                    _logger.LogWarning($"Retry attempt number {retryAttempt} after {timespan.TotalSeconds} seconds due to `{statusCode}` Status Code");
                }
            );
    
    public async Task<BankPaymentResponse> MakePayment(BankPaymentRequest request)
    {
        HttpResponseMessage response = await GetRetryPolicy(_logger).ExecuteAsync(() => _client.PostAsJsonAsync(
            "payments", request, _options));
        
        // We throw on non-200 code
        // Even though the service might return 400,
        // a 400 implies that our code has not validated correctly
        // So from the users perspective, it is a 500
        response.EnsureSuccessStatusCode(); 

        var bankPaymentResponse = await response.Content.ReadFromJsonAsync<BankPaymentResponse>(_options);
        
        if (bankPaymentResponse is null)
            throw new ApiException(ErrorSummary.InternalError, "Bank Service returned null", 500);
        
        return bankPaymentResponse;
    }
}