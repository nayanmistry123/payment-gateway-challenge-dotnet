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

    public PaymentsController(IPaymentsApi paymentsApi)
    {
        _paymentsApi = paymentsApi;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse>> GetPaymentAsync(Guid id)
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
            switch (apiException.StatusCode)
            {
                case 404:
                    return new NotFoundObjectResult(ErrorResponse.FromApiException(apiException));
                case 400:
                    return new BadRequestObjectResult(ErrorResponse.FromApiException(apiException));
            }
        }
        
        Console.WriteLine($"Encountered unexpected exception of type {exception.GetType()}. Exception message: {exception.Message}");
        return StatusCode(500, ErrorResponse.ForUnexpectedException());
    }
    
}