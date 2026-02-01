using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Api;
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
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsApi.GetPayment(id);

        return new OkObjectResult(payment);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> ProcessPayment(PostPaymentRequest postPaymentRequest)
    {
        var payment = _paymentsApi.MakePayment(postPaymentRequest);

        return new OkObjectResult(payment);
    }
}