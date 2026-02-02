using System.Text.Json;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Exceptions;

using Polly;
using Polly.Retry;

namespace PaymentGateway.Api.Services;

public class BankService : IBankService
{
    private readonly HttpClient _client;
    
    //ensure we serialise objects with snake case
    private JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public BankService(HttpClient httpClient)
    {
        _client = httpClient;
    }

    //Retry on any 500 errors, as they may be transient
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy =
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(msg => ((int)msg.StatusCode >= 500 && (int)msg.StatusCode <= 599)) 
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var statusCode = outcome.Result?.StatusCode;
                    Console.WriteLine($"Retry attempt number {retryAttempt} after {timespan.TotalSeconds} seconds due to `{statusCode}` Status Code");
                }
            );
    
    public async Task<BankPaymentResponse> MakePayment(BankPaymentRequest request)
    {
        HttpResponseMessage response = await _retryPolicy.ExecuteAsync(() => _client.PostAsJsonAsync(
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