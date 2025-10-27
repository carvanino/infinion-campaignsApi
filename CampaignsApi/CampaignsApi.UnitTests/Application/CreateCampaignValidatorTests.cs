namespace CampaignsApi.UnitTests.Application;

using CampaignsApi.Application.DTOs;
using CampaignsApi.Application.Validators;
using FluentAssertions;
using Xunit;

public class CreateCampaignValidatorTests
{
    private readonly CreateCampaignValidator _validator;

    public CreateCampaignValidatorTests()
    {
        _validator = new CreateCampaignValidator();
    }

    [Fact]
    public void Validate_WithValidDto_ShouldPass()
    {
        // Arrange
        var dto = new CreateCampaignDto
        {
            Name = "Test Campaign",
            Description = "Test Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(30),
            Budget = 10000
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidName_ShouldFail(string name)
    {
        // Arrange
        var dto = new CreateCampaignDto
        {
            Name = name,
            Description = "Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(30),
            Budget = 10000
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithBudgetZero_ShouldFail()
    {
        // Arrange
        var dto = new CreateCampaignDto
        {
            Name = "Test",
            Description = "Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(30),
            Budget = 0
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Budget");
    }

    [Fact]
    public void Validate_WithBudgetExceedingMax_ShouldFail()
    {
        // Arrange
        var dto = new CreateCampaignDto
        {
            Name = "Test",
            Description = "Description",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(30),
            Budget = 150_000_000  // Over 100M limit
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "Budget" && 
            e.ErrorMessage.Contains("100,000,000"));
    }
}