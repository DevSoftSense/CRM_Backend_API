using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using CRM.Domain.DTOs.Auth.Responses;
using CRM.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Data;

/// <summary>
/// SoftOnCloud product-connection for logged-in CRM staff (Bearer JWT).
/// Never log full connection strings or product service keys.
/// </summary>
public sealed class SoftOnCloudTenantConnectionFactory : ITenantConnectionFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SoftOnCloudTenantConnectionFactory> _logger;
    private readonly AsyncLocal<string?> _requestCache = new();

    public SoftOnCloudTenantConnectionFactory(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SoftOnCloudTenantConnectionFactory> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetResolvedConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_requestCache.Value))
        {
            return _requestCache.Value!;
        }

        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader)
            || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Missing SoftOnCloud bearer token.");
        }

        var productId = ResolveProductId();
        var client = _httpClientFactory.CreateClient("SoftOnCloud");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/auth/product-connection?productId={productId}");
        request.Headers.TryAddWithoutValidation("Authorization", authHeader);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException("SoftOnCloud session ended.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException("Product connection unavailable for this user.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "SoftOnCloud product-connection failed. Status={StatusCode} ProductId={ProductId} BodyLength={Length}",
                (int)response.StatusCode,
                productId,
                body?.Length ?? 0);
            throw new InvalidOperationException(
                $"SoftOnCloud product-connection failed with status {(int)response.StatusCode}.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ProductConnectionResponse>(
            stream,
            JsonOptions,
            cancellationToken);

        var connectionString = payload?.ResolvedConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Empty product connection from SoftOnCloud.");
        }

        _logger.LogInformation(
            "SoftOnCloud product-connection resolved DB for ProductId={ProductId} FromCache={FromCache}",
            productId,
            payload?.FromCache ?? false);

        _requestCache.Value = connectionString;
        return connectionString;
    }

    private int ResolveProductId()
    {
        // SoftOnCloud Auth v2.5: productId = configured app-id only (CRM = 28). Never from browser.
        return _configuration.GetValue<int?>("CRM:AppId")
               ?? _configuration.GetValue<int?>("CRM:ProductId")
               ?? _configuration.GetValue<int?>("SoftOnCloud:AppId")
               ?? _configuration.GetValue<int?>("SoftOnCloud:ProductId")
               ?? throw new InvalidOperationException(
                   "CRM SoftOnCloud app-id is not configured. Set CRM:AppId / SoftOnCloud:ProductId = 28.");
    }
}
