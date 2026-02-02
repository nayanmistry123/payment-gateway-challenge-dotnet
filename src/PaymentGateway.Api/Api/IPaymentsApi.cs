using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.WebApi.Requests;

namespace PaymentGateway.Api.Api;

public interface IPaymentsApi
{
    PaymentResponse GetPayment(Guid id);

    Task<PaymentResponse> MakePayment(PostPaymentRequest paymentRequest);
}