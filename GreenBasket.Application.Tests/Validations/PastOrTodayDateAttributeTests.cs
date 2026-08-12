using FluentAssertions;
using GreenBasket.Application.Validations;
using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace GreenBasket.Application.Tests.Validations
{
    public class PastOrTodayDateAttributeTests
    {
        private readonly PastOrTodayDateAttribute _attribute = new();

        [Fact]
        public void IsValid_NullValue_ReturnsFalse()
        {
            // Act
            var result = _attribute.IsValid(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValid_NonDateTimeValue_ReturnsFalse()
        {
            // Act
            var result = _attribute.IsValid("2026-08-12");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValid_DefaultDateTime_ReturnsFalse()
        {
            // Arrange (default DateTime = 0001-01-01)
            var defaultDate = default(DateTime);

            // Act
            var result = _attribute.IsValid(defaultDate);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValid_PastDate_ReturnsTrue()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddDays(-5);

            // Act
            var result = _attribute.IsValid(pastDate);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_TodayUtcDate_ReturnsTrue()
        {
            // Arrange
            var todayUtc = DateTime.UtcNow.Date;

            // Act
            var result = _attribute.IsValid(todayUtc);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_FutureDate_ReturnsFalse()
        {
            // Arrange
            var futureDate = DateTime.UtcNow.AddDays(1);

            // Act
            var result = _attribute.IsValid(futureDate);

            // Assert
            Assert.False(result);
        }
    }
}