using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Api;
using PaymentGateway.Api.Models.Exceptions;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

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
    public async Task<ActionResult<GetPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        try
        {
            var payment = _paymentsApi.GetPayment(id);
            return new OkObjectResult(payment);
        }
        catch (ApiException e)
        {
            return HandleApiException(e);
        }
    }
    
    [HttpPost("")]
    public async Task<ActionResult<PostPaymentResponse?>> ProcessPayment(PostPaymentRequest postPaymentRequest)
    {
        var payment = _paymentsApi.MakePayment(postPaymentRequest);

        return new OkObjectResult(payment);
    }

    private ObjectResult HandleApiException(ApiException exception)
    {
        if (exception.StatusCode == 404)
            return new NotFoundObjectResult(exception);
        if (exception.StatusCode == 400)
            return new BadRequestObjectResult(exception);
        throw exception;
    }
    
}