using PaymentGateway.Api.Models.Bank;

namespace PaymentGateway.Api.Services;

public interface IBankService
{
    Task<BankPaymentResponse> MakePayment(BankPaymentRequest request);
}