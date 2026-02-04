namespace PaymentGateway.Api.Models;

/// <summary>
/// Internal Enum for payment status
/// </summary>
public enum PaymentStatus
{
    Authorized,
    Declined,
    Rejected
}