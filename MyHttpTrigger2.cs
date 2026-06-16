using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;

public class MyHttpTrigger2
{
    private readonly ILogger _logger;

    public MyHttpTrigger2(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<MyHttpTrigger2>();
    }

   
[Function("GraphWebhookReceiver")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
    {
        _logger.LogInformation("Webhook triggered");

        // ✅ Parse query parameters
        var query = req.Url.Query;

        // Grab the validationToken URL parameter        
        var queryParams = QueryHelpers.ParseQuery(req.Url.Query);

        string? validationToken = null;

        if (queryParams.TryGetValue("validationToken", out var token))
        {
            validationToken = token.ToString();
        }


        // If a validation token is present, we need to respond within 5 seconds by
        // returning the given validation token. This only happens when a new
        // webhook is being added
        if (validationToken != null)
        {
            _logger.LogInformation($"Validation token {validationToken} received");
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain");
            await response.WriteStringAsync(validationToken);
            return response;
        }

        // ✅ Normal webhook notifications
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        _logger.LogInformation($"Notification received: {requestBody}");

        var okResponse = req.CreateResponse(HttpStatusCode.OK);
        await okResponse.WriteStringAsync("OK");
        return okResponse;
    }
}

