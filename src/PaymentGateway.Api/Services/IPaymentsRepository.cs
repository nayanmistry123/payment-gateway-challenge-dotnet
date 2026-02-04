using PaymentGateway.Api.Models.Internal;

namespace PaymentGateway.Api.Services;

public interface IPaymentsRepository
{
    /// <summary>
    /// Adds a payment to the database
    /// </summary>
    /// <param name="payment">The payment to store</param>
    /// <remarks>Throws an API Exception if the payment already exists</remarks>
    public void Add(Payment payment);

    /// <summary>
    /// Retrieves a payment by its ID.
    /// </summary>
    /// <param name="id">The ID of the payment</param>
    /// <returns>The payment if found, else null</returns>
    public Payment? Get(Guid id);
}