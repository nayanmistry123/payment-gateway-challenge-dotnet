using System.Net;
using System.Text;
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

    [Theory]
    public async Task PostPayment_ThenRetrieve_ReturnsExpectedPayment(bool authorised)
    {
        var cardNumber = "12345678901234" + (authorised ? "5" : "6");
        string jsonToPost = GetPostPaymentJson(
            cardNumber,
            1,
            DateTimeOffset.Now.AddYears(1).Year,
            "GBP",
            1000,
            "0123"
        );
        
        using var content = new StringContent(
            jsonToPost,
            Encoding.UTF8,
            "application/json"
        );

        var client = new HttpClient();
        
        var response = await client.PostAsync(
            "https://localhost:7092/api/Payments",
            content
        );

        string responseJson = await response.Content.ReadAsStringAsync();

        Assert.That(response.IsSuccessStatusCode);
        
        var successResponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseJson);
        Assert.That(successResponse.Count, Is.EqualTo(7));
        
        Assert.That(successResponse["status"].GetString(), Is.EqualTo(authorised ? "Authorized" : "Declined"));
        Assert.That(successResponse["cardNumberLastFour"].GetString(), Is.EqualTo(authorised ? "2345" : "2346"));
        Assert.That(successResponse["expiryMonth"].GetInt32(), Is.EqualTo(1));
        Assert.That(successResponse["expiryYear"].GetInt32(), Is.EqualTo(DateTimeOffset.Now.AddYears(1).Year));
        Assert.That(successResponse["currency"].GetString(), Is.EqualTo("GBP"));
        Assert.That(successResponse["amount"].GetInt32(), Is.EqualTo(1000));
        
        var guid = successResponse["id"].GetGuid();
        var retrievalResponse = await client.GetAsync(
                $"https://localhost:7092/api/Payments/{guid}"
            );
        
        string retrievalResponseJson = await retrievalResponse.Content.ReadAsStringAsync();

        Assert.That(retrievalResponse.IsSuccessStatusCode);
        
        var successRetrievalResponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(retrievalResponseJson);
        Assert.That(successRetrievalResponse.Count, Is.EqualTo(7));
        
        Assert.That(successRetrievalResponse["id"].GetGuid(), Is.EqualTo(guid));
        Assert.That(successRetrievalResponse["status"].GetString(), Is.EqualTo(authorised ? "Authorized" : "Declined"));
        Assert.That(successRetrievalResponse["cardNumberLastFour"].GetString(), Is.EqualTo(authorised ? "2345" : "2346"));
        Assert.That(successRetrievalResponse["expiryMonth"].GetInt32(), Is.EqualTo(1));
        Assert.That(successRetrievalResponse["expiryYear"].GetInt32(), Is.EqualTo(DateTimeOffset.Now.AddYears(1).Year));
        Assert.That(successRetrievalResponse["currency"].GetString(), Is.EqualTo("GBP"));
        Assert.That(successRetrievalResponse["amount"].GetInt32(), Is.EqualTo(1000));
    }

    private string GetPostPaymentJson(
        string cardNumber,
        int expiryMonth,
        int expiryYear,
        string currency,
        int amount,
        string cvv)
    {
       return 
           "{ "
            + $" \"cardnumber\" : \"{cardNumber}\", "
            + $" \"expiryMonth\" : {expiryMonth}, "
            + $" \"expiryYear\" : {expiryYear}, "
            + $" \"currency\" : \"{currency}\", "
            + $" \"amount\" : {amount}, "
            + $" \"cvv\" : \"{cvv}\""
            +" }";
    }
}