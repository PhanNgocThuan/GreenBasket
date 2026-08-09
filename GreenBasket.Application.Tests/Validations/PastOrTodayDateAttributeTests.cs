using FluentAssertions;
using GreenBasket.Application.Validations;
using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace GreenBasket.Application.Tests.Validations
{
    public class PastOrTodayDateAttributeTests
    {
        private readonly PastOrTodayDateAttribute _attribute;

        public PastOrTodayDateAttributeTests()
        {
            _attribute = new PastOrTodayDateAttribute();
        }

        [Fact]
        public void IsValid_ShouldReturnSuccess_WhenDateIsTodayOrInPast()
        {
            // Arrange
            var today = DateTime.Today;
            var pastDate = DateTime.Today.AddDays(-5);
            var validationContext = new ValidationContext(new object());

            // Act
            var todayResult = _attribute.GetValidationResult(today, validationContext);
            var pastResult = _attribute.GetValidationResult(pastDate, validationContext);

            // Assert
            todayResult.Should().Be(ValidationResult.Success);
            pastResult.Should().Be(ValidationResult.Success);
        }

        [Fact]
        public void IsValid_ShouldReturnError_WhenDateIsInFuture()
        {
            // Arrange
            var futureDate = DateTime.Today.AddDays(1);
            var validationContext = new ValidationContext(new object());

            // Act
            var result = _attribute.GetValidationResult(futureDate, validationContext);

            // Assert
            result.Should().NotBe(ValidationResult.Success);
        }
    }
}