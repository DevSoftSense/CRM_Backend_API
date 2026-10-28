using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CRM.Application.Helpers;
using CRM.Application.Services.Interfaces;
using CRM.Domain.DTOs.Auth.Requests;
using CRM.Domain.DTOs.Auth.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Application.Services.Classes;

/// <summary>
/// SoftOnCloud Auth API v2.5 integration (same everyday flow as Appointment).
/// Login + resolve-context with productId = CRM AppId from config (28) — never from browser.
/// </summary>
public sealed class SoftOnCloudAuthService : ISoftOnCloudAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SoftOnCloudAuthService> _logger;

    public SoftOnCloudAuthService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SoftOnCloudAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.OrgId <= 0)
            return Fail("Organisation id (orgId) is required.", "VALIDATION");
        if (string.IsNullOrWhiteSpace(request.Email))
            return Fail("Email is required.", "VALIDATION");
        if (string.IsNullOrWhiteSpace(request.Password))
            return Fail("Password is required.", "VALIDATION");

        var productId = SoftOnCloudProductId.Require(_configuration);
        var client = _httpClientFactory.CreateClient("SoftOnCloud");

        // ── 1) SoftOnCloud POST /api/auth/login ──────────────────────────────
        SoftOnCloudLoginApiResponse login;
        string loginJson;
        using (var loginHttp = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login"))
        {
            var body = new
            {
                orgId = request.OrgId,
                email = request.Email.Trim(),
                password = request.Password,
                rememberMe = request.RememberMe
            };
            loginHttp.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json");

            using var loginResponse = await client.SendAsync(loginHttp, cancellationToken);
            loginJson = await loginResponse.Content.ReadAsStringAsync(cancellationToken);

            if (loginResponse.StatusCode is HttpStatusCode.Unauthorized
                or HttpStatusCode.BadRequest
                or HttpStatusCode.Forbidden)
            {
                return Fail(
                    TryReadMessage(loginJson, "Unable to sign in. Please check your credentials."),
                    "LOGIN_FAILED");
            }

            if (!loginResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "SoftOnCloud login failed. Status={Status} OrgId={OrgId}",
                    (int)loginResponse.StatusCode,
                    request.OrgId);
                return Fail(
                    TryReadMessage(loginJson, "Unable to sign in. Please try again."),
                    "LOGIN_FAILED");
            }

            login = JsonSerializer.Deserialize<SoftOnCloudLoginApiResponse>(loginJson, JsonOptions)
                    ?? new SoftOnCloudLoginApiResponse();
        }

        if (string.IsNullOrWhiteSpace(login.Token))
            return Fail("Login succeeded but no access token was returned.", "LOGIN_FAILED");

        // ── 2) SoftOnCloud GET /api/auth/resolve-context?productId={app-id} ─
        // productId ALWAYS from server config (CRM AppId = 28) — SoftOnCloud Auth guide rule.
        var (ctxStatus, ctxBody, ctxMessage) = await ResolveContextAsync(
            login.Token,
            cancellationToken);

        if (ctxStatus == (int)HttpStatusCode.Unauthorized)
        {
            return Fail(
                ctxMessage ?? "Your session has ended. Please sign in again.",
                "LOGIN_FAILED");
        }

        if (ctxStatus == (int)HttpStatusCode.Forbidden || ctxBody is null)
        {
            var denial = TryReadDenialReason(ctxBody) ?? TryReadDenialReasonFromJson(ctxMessage);
            return new LoginResponse
            {
                Ok = false,
                AccessGranted = false,
                DenialReason = denial,
                AppId = productId,
                ProductId = productId,
                Code = "ACCESS_DENIED",
                Message = ctxMessage
                           ?? (string.IsNullOrWhiteSpace(denial)
                               ? "You do not have access to the CRM product."
                               : $"Access denied: {denial}")
            };
        }

        var accessGranted = true;
        string? denialReason = null;
        if (ctxBody.Value.ValueKind == JsonValueKind.Object)
        {
            if (ctxBody.Value.TryGetProperty("accessGranted", out var ag))
                accessGranted = ag.ValueKind == JsonValueKind.True
                                || (ag.ValueKind == JsonValueKind.String
                                    && bool.TryParse(ag.GetString(), out var b) && b);
            if (ctxBody.Value.TryGetProperty("denialReason", out var dr)
                && dr.ValueKind == JsonValueKind.String)
                denialReason = dr.GetString();
        }

        if (!accessGranted)
        {
            return new LoginResponse
            {
                Ok = false,
                AccessGranted = false,
                DenialReason = denialReason,
                AppId = productId,
                ProductId = productId,
                Code = "ACCESS_DENIED",
                Message = string.IsNullOrWhiteSpace(denialReason)
                    ? "You do not have access to the CRM product."
                    : $"Access denied: {denialReason}"
            };
        }

        return new LoginResponse
        {
            Ok = true,
            Token = login.Token,
            RefreshToken = login.RefreshToken,
            ExpiresAt = login.ExpiresAt,
            SessionId = login.SessionId?.ToString()?.Trim('"'),
            RememberMe = login.RememberMe,
            AppId = productId,
            ProductId = productId,
            AccessGranted = true,
            DenialReason = null,
            User = login.User,
            ProductContext = ctxBody,
            Message = "Signed in successfully.",
            Code = "OK"
        };
    }

    public async Task<(int StatusCode, JsonElement? Body, string? Message)> ResolveContextAsync(
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bearerToken))
            return ((int)HttpStatusCode.Unauthorized, null, "Missing SoftOnCloud bearer token.");

        var productId = SoftOnCloudProductId.Require(_configuration);
        var client = _httpClientFactory.CreateClient("SoftOnCloud");
        var token = StripBearer(bearerToken);

        using var ctxHttp = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/auth/resolve-context?productId={productId}");
        ctxHttp.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        ctxHttp.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var ctxResponse = await client.SendAsync(ctxHttp, cancellationToken);
        var ctxJson = await ctxResponse.Content.ReadAsStringAsync(cancellationToken);
        var status = (int)ctxResponse.StatusCode;

        if (!ctxResponse.IsSuccessStatusCode)
        {
            if (status != (int)HttpStatusCode.Forbidden && status != (int)HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning(
                    "SoftOnCloud resolve-context failed. Status={Status} ProductId={ProductId}",
                    status,
                    productId);
            }

            return (status, TryParseElement(ctxJson), TryReadMessage(ctxJson, null));
        }

        return (status, TryParseElement(ctxJson), null);
    }

    public async Task<(int StatusCode, JsonElement? Body)> SessionStatusAsync(
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bearerToken))
            return ((int)HttpStatusCode.Unauthorized, null);

        var client = _httpClientFactory.CreateClient("SoftOnCloud");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/session-status");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", StripBearer(bearerToken));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return ((int)response.StatusCode, TryParseElement(json));
    }

    public async Task<object?> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("SoftOnCloud");
        using var response = await client.GetAsync("/api/auth/countries", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Unable to load country list from SoftOnCloud.");

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<object>(stream, JsonOptions, cancellationToken);
    }

    private static LoginResponse Fail(string message, string code) => new()
    {
        Ok = false,
        AccessGranted = false,
        Message = message,
        Code = code
    };

    private static string StripBearer(string value)
    {
        var t = value.Trim();
        if (t.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return t["Bearer ".Length..].Trim();
        return t;
    }

    private static JsonElement? TryParseElement(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    private static string? TryReadMessage(string? json, string? fallback)
    {
        if (string.IsNullOrWhiteSpace(json)) return fallback;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var msg)
                && msg.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(msg.GetString()))
            {
                return msg.GetString();
            }
        }
        catch
        {
            /* ignore */
        }

        return fallback;
    }

    private static string? TryReadDenialReason(JsonElement? body)
    {
        if (body is null || body.Value.ValueKind != JsonValueKind.Object) return null;
        if (body.Value.TryGetProperty("denialReason", out var dr)
            && dr.ValueKind == JsonValueKind.String)
            return dr.GetString();
        return null;
    }

    private static string? TryReadDenialReasonFromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("denialReason", out var dr)
                && dr.ValueKind == JsonValueKind.String)
                return dr.GetString();
        }
        catch
        {
            /* ignore */
        }

        return null;
    }
}
