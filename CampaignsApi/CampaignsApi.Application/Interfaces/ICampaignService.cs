namespace CampaignsApi.Application.Interfaces;

using CampaignsApi.Application.DTOs;
using CampaignsApi.Domain.Enums;

public interface ICampaignService {

    Task<CampaignResponseDto?> GetCampaignByIdAsync(Guid id);

    Task<(List<CampaignResponseDto> campaigns, int totalCount, string? continuationToken)> GetCampaignsAsync(
        int pageSize = 20,
        string? continuationToken = null,
        string? nameFilter = null,
        CampaignStatus? statusFilter = null,
        string? sortBy = null,
        bool sortDescending = false
    );

    Task<CampaignResponseDto> CreateCampaignAsync(CreateCampaignDto campaignDTO, string createdBy);

    Task<CampaignResponseDto?> UpdateCampaignAsync(Guid id, UpdateCampaignDto campaignDTO);

    Task<bool> DeleteCampaignAsync(Guid id);
}