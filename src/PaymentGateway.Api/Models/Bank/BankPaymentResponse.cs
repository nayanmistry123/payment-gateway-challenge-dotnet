namespace PaymentGateway.Api.Models.Bank;

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