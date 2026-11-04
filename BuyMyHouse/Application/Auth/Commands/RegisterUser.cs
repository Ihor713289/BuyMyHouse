using BuyMyHouse.Application.Common.DTOs;
using MediatR;

namespace BuyMyHouse.Application.Auth.Commands;

public record RegisterUser(RegisterRequest Request) : IRequest<AuthResponse>;
