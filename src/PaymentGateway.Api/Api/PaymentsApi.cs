using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Exceptions;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.WebApi.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Api;

public class PaymentsApi : IPaymentsApi
{
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly IBankService _bankService;
    private readonly ILogger<PaymentsApi> _logger;

    public PaymentsApi(IPaymentsRepository paymentsRepository, IBankService bankService, ILogger<PaymentsApi> logger)
    {
        _paymentsRepository = paymentsRepository;
        _bankService = bankService;
        _logger = logger;
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

    public async Task<PaymentResponse> MakePayment(PostPaymentRequest paymentRequest)
    {
        DtoValidator.ValidatePostPaymentRequest(paymentRequest);

        var makePayment = paymentRequest.FromWebDto();
        
        var bankServiceResponse = await _bankService.MakePayment(makePayment.ToBankDto());

        var confirmedPayment = makePayment.ToCompletePayment(bankServiceResponse);

        try
        {
            //TODO could add database retries here
            _paymentsRepository.Add(confirmedPayment);
        }
        catch (Exception e)
        {
            //If we fail to save, then we are no longer in sync with the bank, so we need a critical error
            _logger.LogCritical(e, 
                $"Unknown error when trying to save confirmed payment with id {confirmedPayment.Id} and authorisation code {bankServiceResponse.AuthorizationCode}");
            throw new ApiException(
                ErrorSummary.InternalError,
                "An unknown error occurred. Do not attempt this payment again. Please contact support",
                500
            );
        }
        
        return confirmedPayment.ToWebDto();
    }

}