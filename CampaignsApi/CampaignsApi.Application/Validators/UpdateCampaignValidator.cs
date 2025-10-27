namespace CampaignsApi.Application.Validators;

using FluentValidation;
using CampaignsApi.Application.DTOs;


public class UpdateCampaignValidator : AbstractValidator<UpdateCampaignDto> {
    public UpdateCampaignValidator() {

        When(x => x.Name != null, () => {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Campaign name cannot be empty")
                .MaximumLength(200)
                .WithMessage("Campaign name must not exceed 200 characters");
        });

        When(x => x.Description != null, () => {
            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Campaign description cannot be empty")
                .MaximumLength(1000)
                .WithMessage("Campaign description must not exceed 1000 characters");
        });

        When(x => x.StartDate.HasValue, () => {
            RuleFor(x => x.StartDate)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Start date cannot be in the past");
        });

        When(x => x.EndDate.HasValue, () => {
            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate ?? DateTime.MinValue)  // Compare against start date if both provided
                .WithMessage("End date must be after start date");
        });

        When(x => x.Budget.HasValue, () => {
            RuleFor(x => x.Budget)
                .GreaterThan(0)
                .WithMessage("Budget must be greater than zero")
                .LessThanOrEqualTo(100_000_000)  // Add this line
                .WithMessage("Budget cannot exceed 100,000,000");
        });

        When(x => x.Status.HasValue, () => {
            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid campaign status");
        });
    }
}