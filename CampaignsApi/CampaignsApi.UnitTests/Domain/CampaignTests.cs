namespace CampaignsApi.UnitTests.Domain;

using CampaignsApi.Domain.Entities;
using CampaignsApi.Domain.Enums;
using CampaignsApi.Domain.Exceptions;
using FluentAssertions;
using Xunit;


public class CampaignTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCampaign()
    {
        // Arrange
        var name = "Test Campaign";
        var description = "Test Description";
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var budget = 10000.0;
        var createdBy = "test@example.com";

        // Act
        var campaign = Campaign.Create(name, description, startDate, endDate, budget, createdBy);

        // Assert
        campaign.Should().NotBeNull();
        campaign.Id.Should().NotBeEmpty();
        campaign.Name.Should().Be(name);
        campaign.Description.Should().Be(description);
        campaign.StartDate.Should().Be(startDate);
        campaign.EndDate.Should().Be(endDate);
        campaign.Budget.Should().Be(budget);
        campaign.Status.Should().Be(CampaignStatus.Draft);
        campaign.CreatedBy.Should().Be(createdBy);
        campaign.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);

        // Act
        var act = () => Campaign.Create("", "Description", startDate, endDate, 10000, "user");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Campaign name is required");
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldThrowDomainException()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var act = () => Campaign.Create("Name", "Description", startDate, endDate, 10000, "user");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("End date must be after start date");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Create_WithInvalidBudget_ShouldThrowDomainException(double budget)
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);

        // Act
        var act = () => Campaign.Create("Name", "Description", startDate, endDate, budget, "user");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Budget must be greater than zero");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateCampaign()
    {
        // Arrange
        var campaign = Campaign.Create(
            "Original", 
            "Original Description", 
            DateTime.UtcNow, 
            DateTime.UtcNow.AddDays(30), 
            10000, 
            "user"
        );
        
        var newName = "Updated Name";
        var newBudget = 20000.0;

        // Act
        campaign.Update(
            newName, 
            "Updated Description", 
            campaign.StartDate, 
            campaign.EndDate, 
            newBudget, 
            CampaignStatus.Active
        );

        // Assert
        campaign.Name.Should().Be(newName);
        campaign.Budget.Should().Be(newBudget);
        campaign.Status.Should().Be(CampaignStatus.Active);
    }

    [Fact]
    public void SoftDelete_ShouldMarkAsDeleted()
    {
        // Arrange
        var campaign = Campaign.Create(
            "Test",
            "Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(30),
            10000,
            "user"
        );

        // Act
        campaign.SoftDelete();

        // Assert
        campaign.IsDeleted.Should().BeTrue();
    }
}