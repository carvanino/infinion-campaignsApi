namespace CampaignsApi.UnitTests.Application;

using CampaignsApi.Application.DTOs;
using CampaignsApi.Application.Interfaces;
using CampaignsApi.Application.Services;
using CampaignsApi.Domain.Entities;
using CampaignsApi.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

// Unit tests for CampaignService
// Uses Moq to mock repository dependency
public class CampaignServiceTests
{
    private readonly Mock<ICampaignRepository> _mockRepository;
    private readonly CampaignService _service;

    public CampaignServiceTests()
    {
        _mockRepository = new Mock<ICampaignRepository>();
        _service = new CampaignService(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateCampaignAsync_WithValidDto_ShouldReturnCreatedCampaign()
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
        var createdBy = "test@example.com";

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<Campaign>()))
            .ReturnsAsync((Campaign c) => c);

        // Act
        var result = await _service.CreateCampaignAsync(dto, createdBy);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(dto.Name);
        result.Description.Should().Be(dto.Description);
        result.Budget.Should().Be(dto.Budget);
        result.Status.Should().Be(CampaignStatus.Draft);
        result.CreatedBy.Should().Be(createdBy);
        
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Campaign>()), Times.Once);
    }

    [Fact]
    public async Task GetCampaignByIdAsync_WhenExists_ShouldReturnCampaign()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var campaign = Campaign.Create(
            "Test", 
            "Description", 
            DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(30), 
            10000, 
            "user"
        );

        _mockRepository
            .Setup(r => r.GetByIdAsync(campaignId))
            .ReturnsAsync(campaign);

        // Act
        var result = await _service.GetCampaignByIdAsync(campaignId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetCampaignByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        
        _mockRepository
            .Setup(r => r.GetByIdAsync(campaignId))
            .ReturnsAsync((Campaign?)null);

        // Act
        var result = await _service.GetCampaignByIdAsync(campaignId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCampaignAsync_WithPartialUpdate_ShouldMergeCorrectly()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var existingCampaign = Campaign.Create(
            "Original Name", 
            "Original Description", 
            DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(30), 
            10000, 
            "user"
        );

        var updateDto = new UpdateCampaignDto
        {
            Name = "Updated Name",
            Budget = 20000
            // Description, dates, status not provided (should keep original)
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(campaignId))
            .ReturnsAsync(existingCampaign);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Campaign>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateCampaignAsync(campaignId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name"); // Updated
        result.Budget.Should().Be(20000); // Updated
        result.Description.Should().Be("Original Description"); // Kept original
        
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Campaign>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCampaignAsync_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        var campaignId = Guid.NewGuid();
        var campaign = Campaign.Create(
            "Test", 
            "Description", 
            DateTime.UtcNow.AddDays(1), 
            DateTime.UtcNow.AddDays(30), 
            10000, 
            "user"
        );

        _mockRepository
            .Setup(r => r.GetByIdAsync(campaignId))
            .ReturnsAsync(campaign);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Campaign>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteCampaignAsync(campaignId);

        // Assert
        result.Should().BeTrue();
        campaign.IsDeleted.Should().BeTrue();
    }
}