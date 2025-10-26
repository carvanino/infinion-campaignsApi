namespace CampaignsApi.Application.Validators;

using FluentValidation;
using CampaignsApi.Application.DTOs;

public class CreateCampaignValidator : AbstractValidator<CreateCampaignDto> {
    public CreateCampaignValidator() {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Campaign name is required")
            .MaximumLength(200)
            .WithMessage("Campaign name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Campaign description is required")
            .MaximumLength(200)
            .WithMessage("Campaign name must not exceed 200 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date) 
            .WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage("End date is required")
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.Budget)
            .GreaterThan(0)
            .WithMessage("Budget must be greater than zero");
    }
}