using System.Net;
using System.Text.Json;
using NUnit.Framework;
using PaymentGateway.Api.Models.Exceptions;

namespace PaymentGateway.Api.Tests;

/// <summary>
/// Requires Payments API to be running on localhost:7092
/// and Bank Simulator to be running on localhost:8080
/// </summary>
public class PaymentsEndToEndTests
{
    [Test]
    public async Task GetPayment_ForNonExistentPayment_ReturnsError()
    {
        var client = new HttpClient();

        var nonExistentGuid = Guid.NewGuid();
        var response =
            await client.GetAsync(
                $"https://localhost:7092/api/Payments/{nonExistentGuid}"
            );
        
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        
        string jsonResponse = await response.Content.ReadAsStringAsync();

        var error = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonResponse);
        Assert.That(error.Count, Is.EqualTo(3));
        
        Assert.That(error["errorSummary"].GetString(), Is.EqualTo(ErrorSummary.PaymentNotFound.ToString()));
        Assert.That(error["errorDetail"].GetString(), Is.EqualTo($"Payment with Id {nonExistentGuid} does not exist"));
        Assert.That(error["statusCode"].GetInt32(), Is.EqualTo(404));
    }
}