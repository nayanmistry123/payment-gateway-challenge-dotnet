using PaymentGateway.Api.Models.Exceptions;

namespace PaymentGateway.Api.Models.WebApi.Responses;

public class ErrorResponse
{
    /// <summary>
    /// The summary of the error
    /// </summary>
    public string ErrorSummary { get; }
    
    /// <summary>
    /// The detail of the error
    /// </summary>
    public string ErrorDetail { get; }
    
    /// <summary>
    /// The HTTP status code of the error
    /// </summary>
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