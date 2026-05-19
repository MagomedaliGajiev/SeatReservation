using System.Net.Http.Json;
using System.Text.Json;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Core.HttpCommunication;

public abstract class BaseHttpClient
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;

    protected BaseHttpClient(HttpClient httpClient, ILogger logger)
    {
        HttpClient = httpClient;
        Logger = logger;
    }

    protected async Task<Result<TResponse, Error>> HandleResponseAsync<TResponse>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        try
        {
            Envelope<TResponse>? envelope = await response.Content
                .ReadFromJsonAsync<Envelope<TResponse>>(cancellationToken);

            if (envelope is null)
            {
                Logger.LogError(
                    "Failed to deserialize response. StatusCode: {StatusCode}",
                    response.StatusCode);

                return Error.Failure(
                    "http.deserialization_failed",
                    "Failed to parse service response.");
            }

            if (!response.IsSuccessStatusCode)
            {
                Error error = envelope.Error ?? Error.Failure(
                    "http.unknown_error",
                    $"Service returned {response.StatusCode}");

                Logger.LogWarning(
                    "Service returned unsuccessful status. StatusCode: {StatusCode}, Error: {Error}",
                    response.StatusCode,
                    error.GetMessage());

                return error;
            }

            if (envelope.Error is not null)
            {
                Logger.LogWarning(
                    "Service returned error in envelope. Error: {Error}",
                    envelope.Error.GetMessage());

                return envelope.Error;
            }

            if (envelope.Result is null)
            {
                Logger.LogError("Service returned null result in successful envelope");

                return Error.Failure(
                    "http.null_result",
                    "Service returned null result.");
            }

            return envelope.Result;
        }
        catch (JsonException ex)
        {
            Logger.LogError(
                ex,
                "JSON deserialization error. StatusCode: {StatusCode}",
                response.StatusCode);

            return Error.Failure(
                "http.invalid_json",
                "Service returned invalid JSON.");
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Unexpected error while handling response. StatusCode: {StatusCode}",
                response.StatusCode);

            return Error.Failure(
                "http.response_handling_failed",
                "Failed to handle service response.");
        }
    }
}