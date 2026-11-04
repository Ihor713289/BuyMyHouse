using System.Security.Claims;
using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Application.Mortgage.Commands;
using BuyMyHouse.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyMyHouse.Web.Controllers;

[ApiController]
[Route("api/mortgage-application")]
public class MortgageApplicationController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Consumes("application/json")]
    public async Task<IActionResult> Create([FromBody] MortgageApplicationRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(JwtClaimTypes.Subject);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var id = await sender.Send(new CreateMortgageApplication(userId, request), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { id });
    }
}
