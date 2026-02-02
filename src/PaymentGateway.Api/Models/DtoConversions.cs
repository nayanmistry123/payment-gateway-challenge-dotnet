using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Internal;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Models;

public static class DtoConversions
{
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

    public static BankPaymentRequest ToBankDto(this MakePayment payment)
    {
        return new BankPaymentRequest(
            payment.CardNumber,
            payment.ExpiryDate.ToString(), //TODO format correctly
            payment.Currency,
            payment.Amount,
            payment.Cvv
        );
    }

    public static Payment ToCompletePayment(this MakePayment payment, BankPaymentResponse bankPaymentResponse)
    {
        return new Payment(
            id: payment.Id,
            cardNumber: payment.CardNumber,
            expiryDate: payment.ExpiryDate,
            currency: payment.Currency,
            amount: payment.Amount,
            cvv: payment.Cvv,
            paymentAuthorised: bankPaymentResponse.Authorised,
            authorisationCode: bankPaymentResponse.AuthorisationCode
        );
    }

}