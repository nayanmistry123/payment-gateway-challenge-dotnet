namespace PaymentGateway.Api.Models.Bank;

public class BankPaymentResponse
{
    public bool Authorised { get; set; }
    public string AuthorisationCode { get; set; }

    public BankPaymentResponse(bool authorised, string authorisationCode)
    {
        Authorised = authorised;
        AuthorisationCode = authorisationCode;
    }
}