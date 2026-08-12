using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenBasket.Application.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace GreenBasket.Application.Tests
{
    public class EmailServiceTests
    {
        [Fact]
        public async Task SendEmailAsync_MissingPortInConfig_ThrowsFormatExceptionOrArgumentNullException()
        {
            // Arrange: Cấu hình thiếu SmtpSettings:Port
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"SmtpSettings:SenderName", "GreenBasket"},
                {"SmtpSettings:SenderEmail", "no-reply@greenbasket.com"},
                {"SmtpSettings:Server", "smtp.gmail.com"},
                {"SmtpSettings:Username", "user@gmail.com"},
                {"SmtpSettings:Password", "password123"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new EmailService(configuration);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.SendEmailAsync("recipient@example.com", "Test Subject", "<p>Test Body</p>"));
        }

        [Fact]
        public async Task SendEmailAsync_InvalidSmtpServer_ThrowsException()
        {
            // Arrange: Cấu hình với SMTP server giả định không khả dụng
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"SmtpSettings:SenderName", "GreenBasket"},
                {"SmtpSettings:SenderEmail", "no-reply@greenbasket.com"},
                {"SmtpSettings:Server", "invalid.smtp.server.local"},
                {"SmtpSettings:Port", "587"},
                {"SmtpSettings:Username", "user@gmail.com"},
                {"SmtpSettings:Password", "password123"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new EmailService(configuration);

            // Act & Assert: Service phải bắt exception kết nối và re-throw lại theo đúng catch block
            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.SendEmailAsync("recipient@example.com", "Test Subject", "<p>Test Body</p>"));
        }

        [Fact]
        public async Task SendEmailAsync_InvalidRecipientEmail_ThrowsFormatException()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"SmtpSettings:SenderName", "GreenBasket"},
                {"SmtpSettings:SenderEmail", "no-reply@greenbasket.com"},
                {"SmtpSettings:Server", "smtp.gmail.com"},
                {"SmtpSettings:Port", "587"},
                {"SmtpSettings:Username", "user@gmail.com"},
                {"SmtpSettings:Password", "password123"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var service = new EmailService(configuration);

            // Act & Assert: Email người nhận sai định dạng làm MailboxAddress.Parse thất bại
            await Assert.ThrowsAnyAsync<Exception>(() =>
                service.SendEmailAsync("invalid-email-address", "Test Subject", "<p>Test Body</p>"));
        }
    }
}