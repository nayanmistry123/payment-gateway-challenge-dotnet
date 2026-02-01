namespace PaymentGateway.Api.Models.Exceptions;

public class ApiException : Exception
{
    public ErrorSummary ErrorSummary { get; }
    public string ErrorDetail { get; }

    public ApiException(ErrorSummary errorSummary, string errorDetail)
    {
        ErrorSummary = errorSummary;
        ErrorDetail = errorDetail;
    }
}