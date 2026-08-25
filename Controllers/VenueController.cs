using Microsoft.AspNetCore.Mvc;
using TicketApp.DTOs;
using TicketApp.Models;
using TicketApp.Services;

namespace TicketApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenueController : ControllerBase
{
    private readonly IVenueService _service;

    public VenueController(IVenueService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetVenues()
    {
        var venues = await _service.GetAllAsync();

        return Ok(venues);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVenue(int id)
    {
        var venue = await _service.GetByIdAsync(id);

        if (venue == null)
        {
            return NotFound();
        }

        return Ok(venue);
    }

    [HttpPost]
    public async Task<IActionResult> CreateVenue(CreateVenueDto newVenue)
    {
        var venue = new Venue
        {
            Name = newVenue.Name,
            Blocks = newVenue.Blocks.Select(block => new VenueBlock
            {
                Name = block.Name,
                Capacity = block.Capacity
            }).ToList()
        };

        var createdVenue = await _service.CreateAsync(venue);

        return CreatedAtAction(
            nameof(GetVenue),
            new { id = createdVenue.Id },
            createdVenue
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVenue(
        int id,
        UpdateVenueDto updatedVenue)
    {
        var venue = new Venue
        {
            Name = updatedVenue.Name,
            Blocks = updatedVenue.Blocks.Select(block => new VenueBlock
            {
                Name = block.Name,
                Capacity = block.Capacity
            }).ToList()
        };

        var result = await _service.UpdateAsync(id, venue);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVenue(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}