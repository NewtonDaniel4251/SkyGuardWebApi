using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SkyGuard.Core.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkyGuard.Infrastructure.Services
{
    public class AzureAdTokenService : IAzureAdTokenService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<AzureAdTokenService> _logger;

        public AzureAdTokenService(IConfiguration config, ILogger<AzureAdTokenService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> ValidateTokenAsync(string token)
        {
            var tenantId = _config["AzureAd:TenantId"];
            var clientId = _config["AzureAd:ClientId"];
            var authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";

            var retriever = new HttpDocumentRetriever { RequireHttps = true };
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{authority}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                retriever)
            {
                AutomaticRefreshInterval = TimeSpan.FromHours(12),
                RefreshInterval = TimeSpan.FromMinutes(5)
            };

            var config = await configurationManager.GetConfigurationAsync();

            var validationParameters = new TokenValidationParameters
            {
                // Accept all common Azure AD issuer formats
                ValidIssuers = new[]
                {
                    $"https://login.microsoftonline.com/{tenantId}/v2.0",
                    $"https://login.microsoftonline.com/{tenantId}/",
                    $"https://sts.windows.net/{tenantId}/"
                },
                ValidAudiences = new[]
                {
                    _config["AzureAd:ClientId"],
                    $"api://{_config["AzureAd:ClientId"]}"
                },
                IssuerSigningKeys = config.SigningKeys,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principal = handler.ValidateToken(token, validationParameters, out _);
                // Patch preferred_username into ClaimTypes.Email if missing
                var email = principal.Claims.FirstOrDefault(c =>
                             c.Type == ClaimTypes.Email ||
                             c.Type == "email" ||
                             c.Type == "preferred_username" ||
                             c.Type == ClaimTypes.Name ||
                             c.Type == "upn" ||
                             c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value;


                if (!string.IsNullOrEmpty(email) && principal.FindFirst(ClaimTypes.Email) == null)
                {
                    var claims = new List<Claim>(principal.Claims)
                    {
                        new Claim(ClaimTypes.Email, email)
                    };

                    var identity = new ClaimsIdentity(
                        claims,
                        principal.Identity?.AuthenticationType,
                        ClaimTypes.Name,
                        ClaimTypes.Role);

                    return new ClaimsPrincipal(identity);
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure AD token validation failed.");
                return null;
            }
        }
    }
}
