using BuyMyHouse.Application.Common.DTOs;
using MediatR;

namespace BuyMyHouse.Application.Mortgage.Commands;

// UserId is the authenticated user's id (the token's "sub" claim), never taken from the request body.
public record CreateMortgageApplication(string UserId, MortgageApplicationRequest Request) : IRequest<Guid>;
