using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Exceptions;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Api;

public class PaymentsApi : IPaymentsApi
{
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly IBankService _bankService;

    public PaymentsApi(IPaymentsRepository paymentsRepository, IBankService bankService)
    {
        _paymentsRepository = paymentsRepository;
        _bankService = bankService;
    }
    
    public PaymentResponse GetPayment(Guid id)
    {
        var maybePayment = _paymentsRepository.Get(id);

        if (maybePayment is null)
            throw new ApiException(
                ErrorSummary.PaymentNotFound,
                $"Payment with Id {id} does not exist",
                404
            );

        return maybePayment.ToWebDto();
    }

    public PaymentResponse MakePayment(PostPaymentRequest paymentRequest)
    {
        DtoValidator.ValidatePostPaymentRequest(paymentRequest);

        var makePayment = paymentRequest.FromWebDto();
        
        //TODO think about transaction handling
        var bankServiceResponse = _bankService.MakePayment(makePayment.ToBankDto());

        var confirmedPayment = makePayment.ToCompletePayment(bankServiceResponse);

        _paymentsRepository.Add(confirmedPayment);

        return confirmedPayment.ToWebDto();
    }

}