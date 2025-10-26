namespace CampaignsApi.Application.DTOs;

using CampaignsApi.Domain.Enums;

public class UpdateCampaignDto {
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public double? Budget { get; set; }
    public CampaignStatus? Status { get; set; } 
}