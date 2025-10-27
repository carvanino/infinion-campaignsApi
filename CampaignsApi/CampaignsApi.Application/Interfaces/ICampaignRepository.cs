namespace CampaignsApi.Application.Interfaces;

using CampaignsApi.Domain.Entities;
using CampaignsApi.Domain.Enums;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(Guid id);

    Task<Campaign?> GetByNameAsync(string name);

    Task<(List<Campaign> campaigns, int totalCount, string? continuationToken)> GetAllAsync(
        int pageSize = 20,
        string? continuationToken = null,
        string? nameFilter = null,
        CampaignStatus? statusFilter = null,
        string? sortBy = null,
        bool sortDescending = false
    );

    Task<Campaign> CreateAsync(Campaign campaign);

    Task UpdateAsync(Campaign campaign);

    Task DeleteAsync(Guid id);

    Task<bool> ExistsAsync(Guid id);

}