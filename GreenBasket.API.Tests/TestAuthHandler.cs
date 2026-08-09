using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GreenBasket.API.Tests
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrEmpty(authHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail("No Authorization header found"));
            }

            var token = authHeader.ToString();
            var rawValue = token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? token.Substring(7).Trim()
                : token.Trim();

            // Tách role và userId từ dạng "role|userId"
            var parts = rawValue.Split('|');
            var role = parts.Length > 0 && !string.IsNullOrEmpty(parts[0]) ? parts[0] : "Customer";
            var userId = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? parts[1] : "test-user-id";

            var normalizedRole = char.ToUpper(role[0]) + role.Substring(1).ToLower();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Email, "testuser@example.com"),
                new Claim(ClaimTypes.Role, normalizedRole),
                new Claim(ClaimTypes.Role, role),
                new Claim("role", normalizedRole),
                new Claim("role", role)
            };

            var identity = new ClaimsIdentity(claims, "TestScheme", ClaimTypes.Name, ClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}