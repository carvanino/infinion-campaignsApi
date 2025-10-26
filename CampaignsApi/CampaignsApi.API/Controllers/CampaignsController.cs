namespace CampaignsApi.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CampaignsApi.Application.Interfaces;
using CampaignsApi.Application.DTOs;
using CampaignsApi.Domain.Exceptions;
using CampaignsApi.Domain.Enums;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CampaignsController : ControllerBase {
    private readonly ICampaignService _campaignService;
    private readonly ILogger<CampaignsController> _logger;


    public CampaignsController(
       ICampaignService campaignService,
       ILogger<CampaignsController> logger
   ) {
        _campaignService = campaignService;
        _logger = logger;
    }

    // GET /api/campaigns?pageSize=20
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<CampaignResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedResponse<CampaignResponseDto>>> GetCampaigns(
        [FromQuery] int pageSize = 20,
        [FromQuery] string? continuationToken = null,
        [FromQuery] string? name = null,
        [FromQuery] CampaignStatus? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false

    ) {
        try
        {
            var (campaigns, totalCount, nextToken) = await _campaignService.GetCampaignsAsync(
                pageSize,
                continuationToken,
                name,
                status,
                sortBy,
                sortDescending
            );
            var response = new PaginatedResponse<CampaignResponseDto>
            {
                Data = campaigns,
                TotalCount = totalCount,
                PageSize = pageSize,
                ContinuationToken = continuationToken,
                HasMore = nextToken != null
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retreiving campaigns");
            return StatusCode(500, new { message = "An error occurred while retrieving campaigns" });
        }
    }

    // GET /api/campaigns/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CampaignResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CampaignResponseDto>> GetCampaign(Guid id)
    {
        try
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);

            if (campaign == null)
            {
                return NotFound(new { message = $"Campaign with ID {id} not found" });
            }
            return Ok(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign {CampaignId}", id);
            return StatusCode(500, new { message = "An error occured while retrieving the campaign" });
        }
    }

    // POST /api/campaigns - Creates a new campaign
    [HttpPost]
    [ProducesResponseType(typeof(CampaignResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CampaignResponseDto>> CreateCampaign(
        [FromBody] CreateCampaignDto campaignDto)
    {
        try
        {
            // Get user ID from JWT token claims
            // User.FindFirst() reads claims from the authenticated user's token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value
                      ?? "system";

            // Create campaign
            var campaign = await _campaignService.CreateCampaignAsync(campaignDto, userId);

            // Return 201 Created with Location header pointing to the new resource
            // CreatedAtAction generates: Location: /api/campaigns/{id}
            return CreatedAtAction(
                nameof(GetCampaign),  // Action name to generate URL
                new { id = campaign.Id },  // Route values
                campaign  // Response body
            );
        }
        catch (DomainException ex)
        {
            // Business rule violation - return 400 Bad Request
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating campaign");
            return StatusCode(500, new { message = "An error occurred while creating the campaign" });
        }
    }

    // PUT /api/campaigns/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CampaignResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CampaignResponseDto>> UpdateCampaign(
        Guid id,
        [FromBody] UpdateCampaignDto campaignDto)
    {
        try
        {
            var campaign = await _campaignService.UpdateCampaignAsync(id, campaignDto);

            if (campaign == null)
            {
                return NotFound(new { message = $"Campaign with ID {id} not found" });
            }

            return Ok(campaign);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating campaign {CampaignId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the campaign" });
        }
    }


    // DELETE /api/campaigns/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCampaign(Guid id)
    {
        try
        {
            var result = await _campaignService.DeleteCampaignAsync(id);
            
            if (!result)
            {
                return NotFound(new { message = $"Campaign with ID {id} not found" });
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting campaign {CampaignId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the campaign" });
        }
    }

}

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = [];
    public int TotalCount { get; set; } 
    public int PageSize { get; set; }
    public string? ContinuationToken { get; set; } 
    public bool HasMore { get; set; } 
}