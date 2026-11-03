using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CRM.API.Helpers;

/// <summary>
/// Modern .NET TokenHandler (replaces obsolete ISecurityTokenValidator).
/// Validates SoftOnCloud platform JWT tokens across live and local environments.
/// Decodes the JWT, verifies expiration, and normalizes org_id and user_id claims.
/// </summary>
public sealed class SoftOnCloudTokenHandler : JsonWebTokenHandler
{
    public override Task<TokenValidationResult> ValidateTokenAsync(string token, TokenValidationParameters validationParameters)
    {
        try
        {
            var jwt = ReadJsonWebToken(token);
            if (jwt.ValidTo != DateTime.MinValue && jwt.ValidTo < DateTime.UtcNow.AddMinutes(-5))
            {
                return Task.FromResult(new TokenValidationResult
                {
                    IsValid = false,
                    Exception = new SecurityTokenExpiredException($"Token expired at {jwt.ValidTo}")
                });
            }

            var identity = new ClaimsIdentity(jwt.Claims, "Bearer", "user_id", "role");

            // Normalize org_id
            var orgClaim = identity.FindFirst("org_id")
                        ?? identity.FindFirst("orgId")
                        ?? identity.FindFirst("organisation_id")
                        ?? identity.FindFirst("organization_id");
            if (orgClaim != null && identity.FindFirst("org_id") == null)
            {
                identity.AddClaim(new Claim("org_id", orgClaim.Value));
            }

            // Normalize user_id
            var userClaim = identity.FindFirst("user_id")
                         ?? identity.FindFirst("userId")
                         ?? identity.FindFirst("sub")
                         ?? identity.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim != null && identity.FindFirst("user_id") == null)
            {
                identity.AddClaim(new Claim("user_id", userClaim.Value));
            }

            return Task.FromResult(new TokenValidationResult
            {
                IsValid = true,
                ClaimsIdentity = identity,
                SecurityToken = jwt
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new TokenValidationResult
            {
                IsValid = false,
                Exception = ex
            });
        }
    }
}

