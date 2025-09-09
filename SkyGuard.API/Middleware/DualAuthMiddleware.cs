using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using SkyGuard.Core.Services;
using SkyGuard.Infrastructure.Respositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SkyGuard.API.Middleware
{
    public class DualAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public DualAuthMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication for anonymous endpoints
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var azureAdTokenService = scope.ServiceProvider.GetRequiredService<IAzureAdTokenService>();

                // Try to authenticate with JWT first
                var jwtToken = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (!string.IsNullOrEmpty(jwtToken))
                {
                    try
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var jwtKey = authService.GetJwtKey();

                        // Validate that a proper key is returned
                        if (string.IsNullOrWhiteSpace(jwtKey))
                            throw new InvalidOperationException("JWT key is missing or empty.");

                        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

                        // Validate key length: HMAC-SHA256 needs at least 256 bits (32 bytes)
                        if (keyBytes.Length < 32)
                            throw new SecurityTokenValidationException("JWT key must be at least 256 bits (32 bytes).");

                        var securityKey = new SymmetricSecurityKey(keyBytes);

                        var validationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = securityKey,
                            RequireSignedTokens = true,
                            ValidateIssuer = false,  
                            ValidateAudience = false, 
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero
                        };

                        var principal = tokenHandler.ValidateToken(jwtToken, validationParameters, out SecurityToken validatedToken);

                        context.User = principal;
                    }
                    catch
                    {
                        // JWT validation failed - try Azure AD
                        await TryAzureAdAuthentication(context, jwtToken, azureAdTokenService);
                    }
                }
                else
                {
                    // Try Azure AD cookie authentication
                    if (context.User.Identity?.IsAuthenticated ?? false)
                    {
                        // Azure AD authentication already succeeded
                        await _next(context);
                        return;
                    }
                }
            }

            await _next(context);
        }

        private async Task TryAzureAdAuthentication(HttpContext context, string token, IAzureAdTokenService azureAdTokenService)
        {
            var azureAdPrincipal = await azureAdTokenService.ValidateTokenAsync(token);
            if (azureAdPrincipal != null)
            {
                // Get user from database based on Azure AD claims
                var email = azureAdPrincipal.FindFirstValue(ClaimTypes.Email) ??
                           azureAdPrincipal.FindFirstValue("preferred_username");


                if (!string.IsNullOrEmpty(email))
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                        var user = await userRepository.GetByEmailAsync(email);

                        if (user != null)
                        {
                            // Create a new identity with our application claims
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim(ClaimTypes.Role, user.Role.ToString())
                            };

                            var identity = new ClaimsIdentity(claims, "AzureAD");
                            context.User = new ClaimsPrincipal(identity);
                        }
                    }
                }
            }
        }
    }
}