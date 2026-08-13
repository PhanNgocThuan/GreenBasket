using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenBasket.Application.DTOs.Auth;
using GreenBasket.Application.Interfaces;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace GreenBasket.Application.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly AuthService _authService;
        private readonly IMemoryCache _cache;

        public AuthServiceTests()
        {
            var store = new Mock<IUserStore<AppUser>>();
            _mockUserManager = new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockEmailService = new Mock<IEmailService>();
            _cache = new MemoryCache(new MemoryCacheOptions());

            // Mock cấu hình JWT Secret hợp lệ (tối thiểu 32 ký tự cho HMAC-SHA256)
            _mockConfiguration.Setup(c => c["JWT:Secret"]).Returns("SUPER_SECRET_KEY_FOR_JWT_TOKEN_GENERATION_123456");
            _mockConfiguration.Setup(c => c["JWT:ValidIssuer"]).Returns("GreenBasketIssuer");
            _mockConfiguration.Setup(c => c["JWT:ValidAudience"]).Returns("GreenBasketAudience");

            _authService = new AuthService(_mockUserManager.Object, _mockConfiguration.Object, _mockEmailService.Object, _cache);
        }

        [Fact]
        public async Task RegisterAsync_Success_ReturnsPendingVerificationToken()
        {
            // Arrange
            var registerDto = new RegisterDTO { Email = "test@example.com", FullName = "Test User", Password = "Password123!" };

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), "Customer"))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.GenerateTwoFactorTokenAsync(It.IsAny<AppUser>(), "Email"))
                .ReturnsAsync("123456");

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PendingVerification", result.Token);
            Assert.Equal(registerDto.Email, result.Email);
            _mockEmailService.Verify(e => e.SendEmailAsync(registerDto.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_CreationFailed_ThrowsException()
        {
            // Arrange
            var registerDto = new RegisterDTO { Email = "test@example.com", Password = "123" };
            var identityError = new IdentityError { Description = "Password too short" };

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(registerDto));
            Assert.Contains("Registration failed: Password too short", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_UserNotFound_ThrowsException()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "notfound@example.com", Password = "Password123!" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(loginDto.Email)).ReturnsAsync((AppUser)null!);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(loginDto));
            Assert.Equal("Invalid email or password.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_EmailNotConfirmed_ThrowsException()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "unconfirmed@example.com", Password = "Password123!" };
            var user = new AppUser { Email = loginDto.Email, EmailConfirmed = false };

            _mockUserManager.Setup(m => m.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(loginDto));
            Assert.Equal("Please verify your email before logging in.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsJwtToken()
        {
            // Arrange
            var loginDto = new LoginDTO { Email = "user@example.com", Password = "Password123!" };
            var user = new AppUser { Id = "user-123", Email = loginDto.Email, FullName = "John Doe", EmailConfirmed = true };

            _mockUserManager.Setup(m => m.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(true);
            _mockUserManager.Setup(m => m.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.Id, result.UserId);
        }

        [Fact]
        public async Task VerifyEmailAsync_ValidOtp_ReturnsTrueAndUpdatesUser()
        {
            // Arrange
            var user = new AppUser { Id = "test-id", Email = "test@example.com", EmailConfirmed = false };
            _mockUserManager.Setup(m => m.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.VerifyTwoFactorTokenAsync(user, "Email", "123456")).ReturnsAsync(true);
            _mockUserManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });
            _cache.Set($"OTP_{user.Email}", "123456");

            // Act
            var result = await _authService.VerifyEmailAsync("test@example.com", "123456");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.True(user.EmailConfirmed);
            _mockUserManager.Verify(m => m.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task ResendOtpAsync_AlreadyConfirmed_ThrowsException()
        {
            // Arrange
            var user = new AppUser { Email = "confirmed@example.com", EmailConfirmed = true };
            _mockUserManager.Setup(m => m.FindByEmailAsync("confirmed@example.com")).ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authService.ResendOtpAsync("confirmed@example.com"));
            Assert.Equal("Email is already verified.", exception.Message);
        }

        [Fact]
        public async Task ForgotPasswordAsync_UserNotFound_ReturnsTrueWithoutSendingEmail()
        {
            // Arrange
            _mockUserManager.Setup(m => m.FindByEmailAsync("unknown@example.com")).ReturnsAsync((AppUser)null!);

            // Act
            var result = await _authService.ForgotPasswordAsync("unknown@example.com");

            // Assert
            Assert.True(result);
            _mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResetPasswordAsync_Success_ReturnsTrue()
        {
            // Arrange
            var dto = new ResetPasswordDTO { Email = "test@example.com", Otp = "123456", NewPassword = "NewPassword123!" };
            var user = new AppUser { Id = "test-id", Email = "test@example.com" };
            _mockUserManager.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(m => m.VerifyTwoFactorTokenAsync(user, "Email", dto.Otp)).ReturnsAsync(true);
            _mockUserManager.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
            _mockUserManager.Setup(m => m.ResetPasswordAsync(user, "reset-token", dto.NewPassword)).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });
            _cache.Set($"OTP_{dto.Email}", "123456");

            // Act
            var result = await _authService.ResetPasswordAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            _mockUserManager.Verify(m => m.ResetPasswordAsync(user, "reset-token", dto.NewPassword), Times.Once);
        }
    }
}