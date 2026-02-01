using PaymentGateway.Api.Models.Internal;

namespace PaymentGateway.Api.Services;

public interface IPaymentsRepository
{
    public void Add(Payment payment);

    public Payment? Get(Guid id);
}