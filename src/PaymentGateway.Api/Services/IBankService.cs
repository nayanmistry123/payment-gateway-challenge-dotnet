using PaymentGateway.Api.Models.Bank;

namespace PaymentGateway.Api.Services;

public interface IBankService
{
    /// <summary>
    /// Sends a payment to the Bank Simulator and returns the result
    /// </summary>
    /// <param name="request">The payment to be sent</param>
    /// <returns>The response from the bank</returns>
    /// <remarks>Throws an API Exception if the bank returns a non-200 code</remarks>>
    Task<BankPaymentResponse> MakePayment(BankPaymentRequest request);
}