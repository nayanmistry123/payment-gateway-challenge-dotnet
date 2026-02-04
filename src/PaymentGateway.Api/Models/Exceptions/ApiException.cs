namespace PaymentGateway.Api.Models.Exceptions;

/// <summary>
/// An API Exception thrown by the code.
/// </summary>
public class ApiException : Exception
{
    public ErrorSummary ErrorSummary { get; }
    public string ErrorDetail { get; }
    public int StatusCode { get; }

    public ApiException(ErrorSummary errorSummary, string errorDetail, int statusCode)
    {
        ErrorSummary = errorSummary;
        ErrorDetail = errorDetail;
        StatusCode = statusCode;
    }
}