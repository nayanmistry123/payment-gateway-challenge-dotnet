using PaymentGateway.Api.Models.Bank;

namespace PaymentGateway.Api.Services;

public interface IBankService
{
    BankPaymentResponse MakePayment(BankPaymentRequest request);
}