namespace CampaignsApi.Domain.Entities;

using CampaignsApi.Domain.Enums;
using CampaignsApi.Domain.Exceptions;

public class Campaign
{
    public Guid Id {get; private set;}
    public string Name {get; private set;}
    public string Description {get; private set;}
    public DateTime StartDate {get; private set;}
    public DateTime EndDate {get; private set;}
    public double Budget {get; private set;}
    public CampaignStatus Status {get; private set;}
    public string CreatedBy {get; private set;}
    public DateTime CreatedAt {get; private set;}
    public DateTime UpdatedAt {get; private set;} 
    public bool IsDeleted {get; private set;}

    private Campaign() { 
        Name = null!;
        Description = null!;
        CreatedBy = null!;
    }

    public static Campaign Create(
        string name,
        string description,
        DateTime startDate,
        DateTime endDate,
        double budget, 
        string createdBy
    ) {
        // Validate provided fields
        ValidateName(name);
        ValidateBudget(budget);
        ValidateDates(startDate, endDate);


        return new Campaign {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Budget = budget,
            Status = CampaignStatus.Draft,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public void Update(
        string name,
        string description,
        DateTime startDate,
        DateTime endDate,
        double budget,
        CampaignStatus newStatus
    ) {

        // Validate fields
        ValidateName(name);
        ValidateDates(startDate, endDate);
        ValidateBudget(budget);
        ValidateStatusChange(newStatus);

        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        Budget = budget;
        UpdatedAt = DateTime.UtcNow;
        Status = newStatus;
    }

    public void SoftDelete() {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateDates(DateTime startDate, DateTime endDate) {
        // Business rule: End date must be after start date
        // A campaign can't end before it starts - that's logically impossible
        if (endDate <= startDate) {
            throw new DomainException("End date must be after start date");
        }
    }

    private static void ValidateBudget(double budget) {
        // Business rule: Budget must be positive
        // Can't have a campaign with zero or negative budget
        if (budget <= 0)
        {
            throw new DomainException("Budget must be greater than zero");
        }
        
        if (budget > 100_000_000)
        {
            throw new DomainException("Budget cannot exceed 100,000,000");
        }
    }

    private static void ValidateName(string name) {
        // Business rule: Name is required
        // Every campaign needs a name to identify it
        if (string.IsNullOrWhiteSpace(name)) {
            throw new DomainException("Campaign name is required");
        }
    }

    private void ValidateStatusChange(CampaignStatus newStatus) {
        // Business rule: Can't activate before start date
        if (newStatus == CampaignStatus.Active && StartDate > DateTime.UtcNow) {
            throw new DomainException("Campaign cannot be activated before start date");
        }
    }

}