using System.Text.Json;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Exceptions;

namespace PaymentGateway.Api.Services;

public class BankService : IBankService
{
    private HttpClient _client;
    
    //ensure we serialise objects with snake case
    private JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public BankService()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("http://localhost:8080/");
    }
    
    public async Task<BankPaymentResponse> MakePayment(BankPaymentRequest request)
    {
        //TODO implement retries
        
        HttpResponseMessage response = await _client.PostAsJsonAsync(
            "payments", request, _options);
        
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