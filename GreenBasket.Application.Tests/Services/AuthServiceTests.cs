using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using GreenBasket.Application.DTOs.Auth;
using GreenBasket.Application.Services;
using GreenBasket.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace GreenBasket.Application.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly UserManager<AppUser> _userManagerMock;
        private readonly IConfiguration _configurationMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var userStoreMock = Substitute.For<IUserStore<AppUser>>();
            _userManagerMock = Substitute.For<UserManager<AppUser>>(
                userStoreMock, null!, null!, null!, null!, null!, null!, null!, null!);

            _configurationMock = Substitute.For<IConfiguration>();

            // JWT Key bắt buộc phải dài tối thiểu 32 ký tự (256-bit) cho thuật toán HMAC-SHA256
            _configurationMock["JWT:Secret"].Returns("ThisIsASuperSecretKeyThatIs32BytesLong!");
            _configurationMock["JWT:ValidIssuer"].Returns("GreenBasketAPI");
            _configurationMock["JWT:ValidAudience"].Returns("GreenBasketUsers");

            _authService = new AuthService(_userManagerMock, _configurationMock);
        }

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenUserCreationFails()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                Email = "test@example.com",
                Password = "Password123!",
                FullName = "Test User"
            };

            var error = new IdentityError { Description = "Password is too weak." };
            _userManagerMock.CreateAsync(Arg.Any<AppUser>(), dto.Password)
                             .Returns(IdentityResult.Failed(error));

            // Act
            Func<Task> act = async () => await _authService.RegisterAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("*Registration failed: Password is too weak.*");
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnAuthResponseDTO_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var dto = new RegisterDTO
            {
                Email = "test@example.com",
                Password = "Password123!",
                FullName = "Test User"
            };

            _userManagerMock.CreateAsync(Arg.Any<AppUser>(), dto.Password)
                             .Returns(IdentityResult.Success);

            _userManagerMock.AddToRoleAsync(Arg.Any<AppUser>(), "Customer")
                             .Returns(IdentityResult.Success);

            _userManagerMock.GetRolesAsync(Arg.Any<AppUser>())
                             .Returns(new List<string> { "Customer" });

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(dto.Email);
            result.FullName.Should().Be(dto.FullName);
            result.Token.Should().NotBeNullOrWhiteSpace();

            await _userManagerMock.Received(1).AddToRoleAsync(Arg.Any<AppUser>(), "Customer");
        }

        #endregion

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenEmailDoesNotExist()
        {
            // Arrange
            var dto = new LoginDTO
            {
                Email = "notfound@example.com",
                Password = "Password123!"
            };

            _userManagerMock.FindByEmailAsync(dto.Email)
                             .Returns(Task.FromResult<AppUser?>(null));

            // Act
            Func<Task> act = async () => await _authService.LoginAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("Email does not exist in the system.");
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenPasswordIsIncorrect()
        {
            // Arrange
            var dto = new LoginDTO
            {
                Email = "user@example.com",
                Password = "WrongPassword"
            };

            var fakeUser = new AppUser
            {
                Id = "user-123",
                Email = dto.Email,
                FullName = "Test User"
            };

            _userManagerMock.FindByEmailAsync(dto.Email)
                             .Returns(Task.FromResult<AppUser?>(fakeUser));

            _userManagerMock.CheckPasswordAsync(fakeUser, dto.Password)
                             .Returns(Task.FromResult(false));

            // Act
            Func<Task> act = async () => await _authService.LoginAsync(dto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                     .WithMessage("Incorrect password.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnAuthResponseDTO_WhenCredentialsAreValid()
        {
            // Arrange
            var dto = new LoginDTO
            {
                Email = "user@example.com",
                Password = "CorrectPassword123!"
            };

            var fakeUser = new AppUser
            {
                Id = "user-123",
                Email = dto.Email,
                FullName = "Test User"
            };

            _userManagerMock.FindByEmailAsync(dto.Email)
                             .Returns(Task.FromResult<AppUser?>(fakeUser));

            _userManagerMock.CheckPasswordAsync(fakeUser, dto.Password)
                             .Returns(Task.FromResult(true));

            _userManagerMock.GetRolesAsync(fakeUser)
                             .Returns(Task.FromResult<IList<string>>(new List<string> { "Customer" }));

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(dto.Email);
            result.FullName.Should().Be(fakeUser.FullName);
            result.Token.Should().NotBeNullOrWhiteSpace();
        }

        #endregion
    }
}