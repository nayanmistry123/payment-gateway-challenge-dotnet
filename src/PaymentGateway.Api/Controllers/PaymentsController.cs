using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Api;
using PaymentGateway.Api.Models.Exceptions;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.WebApi.Requests;
using PaymentGateway.Api.Models.WebApi.Responses;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IPaymentsApi  _paymentsApi;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentsApi paymentsApi, ILogger<PaymentsController> logger)
    {
        _paymentsApi = paymentsApi;
        _logger = logger;
    }

    /// <summary>
    /// Retrieve an existing payment
    /// </summary>
    /// <param name="id">The ID of the payment to retrieve</param>
    /// <returns>The Payment if it exists or an error response.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse>> GetPayment(Guid id)
    {
        try
        {
            var payment =  _paymentsApi.GetPayment(id);
            return Ok(payment);
        }
        catch (Exception e)
        {
            return HandleApiException(e);
        }
    }
    
    /// <summary>
    /// Process a payment
    /// </summary>
    /// <param name="postPaymentRequest">The Payment to process</param>
    /// <returns>The resultant processed payment or an error response.</returns>
    [HttpPost("")]
    public async Task<ActionResult<PaymentResponse>> ProcessPayment(PostPaymentRequest postPaymentRequest)
    {
        try
        {
            var payment = await _paymentsApi.MakePayment(postPaymentRequest);
            return Ok(payment);
        }
        catch (Exception e)
        {
            return HandleApiException(e);
        }
    }

    /// <summary>
    /// Sanitise any exceptions by returning an ErrorResponse object
    /// </summary>
    private ObjectResult HandleApiException(Exception exception)
    {
        if (exception is ApiException apiException)
        {
            _logger.LogError($"Returned Api Exception with code {apiException.StatusCode}");
            switch (apiException.StatusCode)
            {
                case 404:
                    return new NotFoundObjectResult(ErrorResponse.FromApiException(apiException));
                case 400:
                    return new BadRequestObjectResult(ErrorResponse.FromApiException(apiException));
            }
        }
        
        _logger.LogCritical(exception, $"Encountered unexpected exception. Exception message: {exception.Message}");

        return StatusCode(500, ErrorResponse.ForUnexpectedException());
    }
    
}