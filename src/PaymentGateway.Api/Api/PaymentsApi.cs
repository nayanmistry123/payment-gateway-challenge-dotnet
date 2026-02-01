using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Api;

public class PaymentsApi : IPaymentsApi
{
    private readonly IPaymentsRepository _paymentsRepository;

    public PaymentsApi(IPaymentsRepository paymentsRepository)
    {
        _paymentsRepository = paymentsRepository;
    }
    
    public GetPaymentResponse GetPayment(Guid id)
    {
        throw new NotImplementedException();
    }

    public PostPaymentResponse MakePayment(PostPaymentRequest payment)
    {
        throw new NotImplementedException();
    }
}