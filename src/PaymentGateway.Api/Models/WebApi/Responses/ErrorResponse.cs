using PaymentGateway.Api.Models.Exceptions;

namespace PaymentGateway.Api.Models.Responses;

public class ErrorResponse
{
    public string ErrorSummary { get; }
    public string ErrorDetail { get; }
    public int StatusCode { get; }

    private ErrorResponse(string errorSummary, string errorDetail, int statusCode)
    {
        ErrorSummary = errorSummary;
        ErrorDetail = errorDetail;
        StatusCode = statusCode;
    }

    public static ErrorResponse FromApiException(ApiException apiException)
    {
        return new ErrorResponse(
            apiException.ErrorSummary.ToString(),
            apiException.ErrorDetail,
            apiException.StatusCode);
    }
    
    public static ErrorResponse ForUnexpectedException()
    {
        return new ErrorResponse(
            Exceptions.ErrorSummary.InternalError.ToString(),
            "An Unexpected Error Occurred",
            500);
    }
}