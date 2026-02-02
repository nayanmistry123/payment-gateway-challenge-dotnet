using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Internal;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Models;

public static class DtoConversions
{
    /// <summary>
    /// Convert an internal Payment DTO to a Web DTO Payment
    /// </summary>
    public static PaymentResponse ToWebDto(this Payment payment)
    {
        return new PaymentResponse(
            id: payment.Id,
            status: payment.PaymentAuthorised ? PaymentStatus.Authorized : PaymentStatus.Declined,
            cardNumberLastFour: payment.CardNumber.Substring(payment.CardNumber.Length - 4),
            expiryMonth: payment.ExpiryDate.Month,
            expiryYear: payment.ExpiryDate.Year,
            currency: payment.Currency,
            amount: payment.Amount
        );
    }
    
    /// <summary>
    /// Convert a PostPaymentRequest Web DTO to an internal payment DTO
    /// </summary>
    public static MakePayment FromWebDto(this PostPaymentRequest request)
    {
        return new MakePayment(
            Guid.NewGuid(),
            cardNumber: request.CardNumber,
            expiryDate: DateTimeOffset.Parse($"{request.ExpiryYear}-{request.ExpiryMonth}-01"),
            currency: request.Currency,
            amount: request.Amount,
            cvv: request.Cvv
        );
    }

    /// <summary>
    /// Convert an internal Payment request to a Bank Web DTO
    /// </summary>
    public static BankPaymentRequest ToBankDto(this MakePayment payment)
    {
        return new BankPaymentRequest(
            payment.CardNumber,
            payment.ExpiryDate.ToString("MM/yyyy"),
            payment.Currency,
            payment.Amount,
            payment.Cvv
        );
    }

    /// <summary>
    /// Create a complete payment from an internal payment request and bank payment response
    /// </summary>
    public static Payment ToCompletePayment(this MakePayment payment, BankPaymentResponse bankPaymentResponse)
    {
        return new Payment(
            id: payment.Id,
            cardNumber: payment.CardNumber,
            expiryDate: payment.ExpiryDate,
            currency: payment.Currency,
            amount: payment.Amount,
            cvv: payment.Cvv,
            paymentAuthorised: bankPaymentResponse.Authorized,
            authorisationCode: bankPaymentResponse.AuthorizationCode
        );
    }
}