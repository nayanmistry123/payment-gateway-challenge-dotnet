using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Api;

public interface IPaymentsApi
{
    PaymentResponse GetPayment(Guid id);

    Task<PaymentResponse> MakePayment(PostPaymentRequest paymentRequest);
}