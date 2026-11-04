using BuyMyHouse.Application.Common.DTOs;
using MediatR;

namespace BuyMyHouse.Application.Auth.Commands;

public record LoginUser(LoginRequest Request) : IRequest<AuthResponse>;
