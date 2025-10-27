namespace CampaignsApi.Application.Services;

using CampaignsApi.Application.DTOs;
using CampaignsApi.Application.Interfaces;
using CampaignsApi.Domain.Entities;
using CampaignsApi.Domain.Enums;
using CampaignsApi.Domain.Exceptions;

public class CampaignService : ICampaignService {
    private readonly ICampaignRepository _repository;

    public CampaignService(ICampaignRepository repository) {
        _repository = repository;
    }

    public async Task<CampaignResponseDto?> GetCampaignByIdAsync(Guid id) {
        var campaign = await _repository.GetByIdAsync(id);

        if (campaign == null || campaign.IsDeleted) {
            return null;
        }
        return MapToDto(campaign);
    }

    public async Task<(List<CampaignResponseDto> campaigns, int totalCount, string? continuationToken)> GetCampaignsAsync(
        int pageSize = 20,
        string? continuationToken = null,
        string? nameFilter = null,
        CampaignStatus? statusFilter = null,
        string? sortBy = null,
        bool sortDescending = false
    ) {
        if (pageSize > 100) {
            pageSize = 100;
        }

        var (campaigns, totalCount, token) = await _repository.GetAllAsync(
            pageSize,
            continuationToken,
            nameFilter,
            statusFilter,
            sortBy,
            sortDescending
        );

        var campaignsDtos = campaigns.Select(MapToDto).ToList();

        return (campaignsDtos, totalCount, token);
    }

    public async Task<CampaignResponseDto> CreateCampaignAsync(CreateCampaignDto campaignDTO, string createdBy) {

        var existingCampaign = await _repository.GetByNameAsync(campaignDTO.Name);

        if (existingCampaign != null)
        {
            throw new DomainException($"A campaign with the name '{campaignDTO.Name}' already exists");
        }
        
        var campaign = Campaign.Create(
            campaignDTO.Name,
            campaignDTO.Description,
            campaignDTO.StartDate,
            campaignDTO.EndDate,
            campaignDTO.Budget,
            createdBy
        );

        var createdCampaign = await _repository.CreateAsync(campaign);

        return MapToDto(createdCampaign);
    }

    public async Task<CampaignResponseDto?> UpdateCampaignAsync(Guid id, UpdateCampaignDto campaignDTO) {
        var campaign = await _repository.GetByIdAsync(id);

        if (campaign == null || campaign.IsDeleted)
        {
            return null;
        }
        
        if (campaignDTO.Name != null && campaignDTO.Name != campaign.Name)
        {
            var existingCampaign = await _repository.GetByNameAsync(campaignDTO.Name);
            
            if (existingCampaign != null && existingCampaign.Id != id)
            {
                throw new DomainException($"A campaign with the name '{campaignDTO.Name}' already exists");
            }
        }

        var updatedName = campaignDTO.Name ?? campaign.Name;
        var updatedDescription = campaignDTO.Description ?? campaign.Description;
        var updatedStartDate = campaignDTO.StartDate ?? campaign.StartDate;
        var updatedEndDate = campaignDTO.EndDate ?? campaign.EndDate;
        var updatedBudget = campaignDTO.Budget ?? campaign.Budget;
        var updatedStatus = campaignDTO.Status ?? campaign.Status;

        campaign.Update(
            updatedName,
            updatedDescription,
            updatedStartDate,
            updatedEndDate,
            updatedBudget,
            updatedStatus
        );

        await _repository.UpdateAsync(campaign);
        
        return MapToDto(campaign);
    }

    public async Task<bool> DeleteCampaignAsync(Guid id) {
        var campaign = await _repository.GetByIdAsync(id);
        
        if (campaign == null || campaign.IsDeleted)
        {
            return false;
        }
        
        campaign.SoftDelete();
        
        await _repository.UpdateAsync(campaign);
        
        return true;
    }


    private static CampaignResponseDto MapToDto(Campaign campaign) {
        return new CampaignResponseDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            Budget = campaign.Budget,
            Status = campaign.Status,
            CreatedBy = campaign.CreatedBy,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt
        };
    }

}