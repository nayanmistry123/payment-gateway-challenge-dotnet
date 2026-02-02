using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Api;
using PaymentGateway.Api.Models.Exceptions;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

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
            return new OkObjectResult(payment);
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
            var payment = _paymentsApi.MakePayment(postPaymentRequest);
            return new OkObjectResult(payment);
        }
        catch (Exception e)
        {
            return HandleApiException(e);
        }
    }

    private ObjectResult HandleApiException(Exception exception)
    {
        if (exception is ApiException apiException)
        {
            if (apiException.StatusCode == 404)
                return new NotFoundObjectResult(apiException);
            if (apiException.StatusCode == 400)
                return new BadRequestObjectResult(apiException);
        }

        Console.WriteLine($"Encountered unexpected exception of type {exception.GetType()}. Exception message: {exception.Message}");
        return StatusCode(500, "An Unexpected Problem Occurred");
    }
    
}