using PaymentGateway.Api.Models.Internal;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Api;

public interface IPaymentsApi
{
    PaymentResponse GetPayment(Guid id);

    PaymentResponse MakePayment(PostPaymentRequest paymentRequest);
}