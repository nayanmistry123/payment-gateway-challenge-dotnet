using PaymentGateway.Api.Models.Exceptions;
using PaymentGateway.Api.Models.Internal;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository : IPaymentsRepository
{
    private Dictionary<Guid, Payment> Payments = new();
    
    public void Add(Payment payment)
    {
        if (Payments.ContainsKey(payment.Id))
            throw new ApiException(ErrorSummary.InternalError, "Encountered Payment Id that already existed", 500);

        Payments[payment.Id] = payment;
    }

    public Payment? Get(Guid id)
    {
        return Payments.GetValueOrDefault(id);
    }
}