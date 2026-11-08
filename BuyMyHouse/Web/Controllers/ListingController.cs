using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Application.Listing.Commands;
using BuyMyHouse.Application.Listing.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyMyHouse.Web.Controllers;

[ApiController]
[Route("api/listings")]
public class ListingController(ISender sender) : ControllerBase
{
    private const long MaxRequestBytes = ImageUpload.MaxCount * ImageUpload.MaxBytes + 1024 * 1024;

    [HttpGet("search")]
    [ProducesResponseType<List<ListingReadResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromQuery] ListingReadRequest request, CancellationToken cancellationToken)
    {
        var listings = await sender.Send(new FilterListingsQuery(request), cancellationToken);
        return Ok(listings);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<ListingReadResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var listing = await sender.Send(new GetListingByIdQuery(id), cancellationToken);
        return Ok(listing);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<IActionResult> Create([FromForm] ListingWriteRequest request, [FromForm] List<IFormFile> images, CancellationToken cancellationToken)
    {
        var uploads = images.Select(file => new ImageUpload(file.OpenReadStream(), file.ContentType, file.Length)).ToList();
        var id = await sender.Send(new CreateListing(request, uploads), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }
}
