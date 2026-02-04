using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.WebApi.Requests;

namespace PaymentGateway.Api.Api;

public interface IPaymentsApi
{
    /// <summary>
    /// Retrieve a payment by its ID
    /// </summary>
    /// <param name="id">The GUID of the payment</param>
    /// <returns>The payment for the given ID.</returns>
    /// <remarks>Throws an API Exception if the payment does not exist</remarks>
    PaymentResponse GetPayment(Guid id);

    /// <summary>
    /// Makes a payment 
    /// </summary>
    /// <param name="paymentRequest">The payment to be made</param>
    /// <returns>The resulting payment made</returns>
    Task<PaymentResponse> MakePayment(PostPaymentRequest paymentRequest);
}