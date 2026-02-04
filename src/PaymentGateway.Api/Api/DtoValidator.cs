using System.Text.RegularExpressions;
using PaymentGateway.Api.Models.Exceptions;
using PaymentGateway.Api.Models.WebApi.Requests;

namespace PaymentGateway.Api.Api;

public static class DtoValidator
{
    private static readonly List<string> ValidCurrencyCodes =["USD", "GBP", "EUR"];
    
    public static void ValidatePostPaymentRequest(PostPaymentRequest request)
    {
        if (request.CardNumber is null 
            || request.CardNumber.Length < 14 || 19 < request.CardNumber.Length 
            || !Regex.IsMatch(request.CardNumber, @"^\d+$"))
            ConstructInvalidPaymentException("Card Number must be 14-19 numeric characters");
        
        if (request.ExpiryMonth < 1 || request.ExpiryMonth > 12)
            ConstructInvalidPaymentException("Expiry Month must be between 1-12");
        
        var currentTime = DateTimeOffset.UtcNow;
        
        if (request.ExpiryYear < currentTime.Year
            || !DateTimeOffset.TryParse($"{request.ExpiryYear}-{request.ExpiryMonth}-01", out var expiryDateTime) 
            || expiryDateTime <= currentTime)
            ConstructInvalidPaymentException("Expiry Month and Year must be in the future");
        
        if (request.Currency is null || !ValidCurrencyCodes.Contains(request.Currency.ToUpper()))
            ConstructInvalidPaymentException($"Currency must be one of: {string.Join( ", ", ValidCurrencyCodes)}");
        
        if (request.Cvv is null || request.Cvv.Length < 3 || 4 < request.Cvv.Length || !Regex.IsMatch(request.Cvv, @"^\d+$"))
            ConstructInvalidPaymentException("Cvv must be 3-4 numeric characters");
        
        if (request.Amount<=0)
            ConstructInvalidPaymentException("Amount must be non-negative");
    }

    private static void ConstructInvalidPaymentException(string errorDetail)
    {
        throw new ApiException(ErrorSummary.InvalidPaymentRequest, errorDetail, 400);
    }
}