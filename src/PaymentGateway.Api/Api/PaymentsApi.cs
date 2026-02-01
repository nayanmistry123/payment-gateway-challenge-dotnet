using PaymentGateway.Api.Models.Exceptions;
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
        var maybePayment = _paymentsRepository.Get(id);

        if (maybePayment is null)
            throw new ApiException(
                ErrorSummary.PaymentNotFound,
                "Payment with Id {id} does not exist",
                404
            );

        return new GetPaymentResponse(
            maybePayment.Id,
            maybePayment.Status,
            maybePayment.CardNumber.Substring(maybePayment.CardNumber.Length - 4),
            maybePayment.ExpiryDate.Month,
            maybePayment.ExpiryDate.Year,
            maybePayment.Currency,
            maybePayment.Amount
        );
    }

    public PostPaymentResponse MakePayment(PostPaymentRequest payment)
    {
        throw new NotImplementedException();
    }
}