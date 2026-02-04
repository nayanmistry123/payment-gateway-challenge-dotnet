namespace PaymentGateway.Api.Models.Bank;

/// <summary>
/// The Response DTO to be used with the BankService
/// </summary>
public class BankPaymentResponse
{
    public bool Authorized { get; set; }
    public string AuthorizationCode { get; set; }

    public BankPaymentResponse(bool authorized, string authorizationCode)
    {
        Authorized = authorized;
        AuthorizationCode = authorizationCode;
    }
}