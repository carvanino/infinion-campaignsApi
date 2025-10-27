namespace CampaignsApi.Infrastructure.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using CampaignsApi.Application.Interfaces;
using CampaignsApi.Domain.Entities;
using CampaignsApi.Domain.Enums;
using System.Text;
using System.Text.Json;


internal class PaginationToken {
    public Guid LastId { get; set; }
    public DateTime LastCreatedAt { get; set; }
}

public class CampaignRepository : ICampaignRepository {
    private readonly ApplicationDbContext _context;

    public CampaignRepository(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<Campaign?> GetByIdAsync(Guid id)
    {
        return await _context.Campaigns.FindAsync(id);
    }
    
    public async Task<Campaign?> GetByNameAsync(string name)
    {
        return await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<(List<Campaign> campaigns, int totalCount, string? continuationToken)> GetAllAsync(
        int pageSize = 20,
        string? continuationToken = null,
        string? nameFilter = null,
        CampaignStatus? statusFilter = null,
        string? sortBy = null,
        bool sortDescending = false
    ) {

        IQueryable<Campaign> query = _context.Campaigns;

        if (!string.IsNullOrWhiteSpace(nameFilter)) {
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{nameFilter}%"));
        }

        if (statusFilter.HasValue) {
            query = query.Where(c => c.Status == statusFilter.Value);
        }

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "name" => sortDescending 
                ? query.OrderByDescending(c => c.Name) 
                : query.OrderBy(c => c.Name),
            
            "budget" => sortDescending 
                ? query.OrderByDescending(c => c.Budget) 
                : query.OrderBy(c => c.Budget),
            
            "startdate" => sortDescending 
                ? query.OrderByDescending(c => c.StartDate) 
                : query.OrderBy(c => c.StartDate),
            
            "enddate" => sortDescending 
                ? query.OrderByDescending(c => c.EndDate) 
                : query.OrderBy(c => c.EndDate),
            
            "createdat" => sortDescending 
                ? query.OrderByDescending(c => c.CreatedAt) 
                : query.OrderBy(c => c.CreatedAt),
            
            // Default: newest first
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        if (!string.IsNullOrWhiteSpace(continuationToken)) {
            try {
                var tokenBytes = Convert.FromBase64String(continuationToken);
                var tokenJson = Encoding.UTF8.GetString(tokenBytes);

                var token = JsonSerializer.Deserialize<PaginationToken>(tokenJson);

                if (token != null) {
                    query = query.Where(c => c.CreatedAt < token.LastCreatedAt ||
                    (c.CreatedAt == token.LastCreatedAt && c.Id != token.LastId));
                }
            }
            catch (Exception) {

            }
        }

        var campaigns = await query.Take(pageSize + 1).ToListAsync();

        string? nextToken = null;
        if (campaigns.Count > pageSize) {
            var lastItem = campaigns[pageSize - 1];

            campaigns = campaigns.Take(pageSize).ToList();

            var token = new PaginationToken {
                LastId = lastItem.Id,
                LastCreatedAt = lastItem.CreatedAt
            };

            var tokenJson = JsonSerializer.Serialize(token);

            nextToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenJson));
        }

        return (campaigns, totalCount, nextToken);
    }

    public async Task<Campaign> CreateAsync(Campaign campaign) {
        await _context.Campaigns.AddAsync(campaign);
        
        await _context.SaveChangesAsync();
        
        return campaign;
    }

    public async Task UpdateAsync(Campaign campaign) {
        _context.Campaigns.Update(campaign);
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id) {
        /* var campaign = await GetByIdAsync(id);
        
        if (campaign != null) {
            campaign.SoftDelete();
            
            await _context.SaveChangesAsync();
        } */
        await _context.Campaigns
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.IsDeleted, true)
                .SetProperty(c => c.UpdatedAt, DateTime.UtcNow));
    }

    public async Task<bool> ExistsAsync(Guid id) {
        return await _context.Campaigns.AnyAsync(c => c.Id == id);
    }
}