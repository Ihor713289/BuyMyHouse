using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Application.Common.Exceptions;
using BuyMyHouse.Application.Common.Services;
using BuyMyHouse.Domain.Entities;
using MediatR;

namespace BuyMyHouse.Application.Auth.Commands.Handlers;

public class RegisterUserHandler(IUserCommands userCommands, IPasswordHasher passwordHasher, ITokenService tokenService)
    : IRequestHandler<RegisterUser, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterUser request, CancellationToken cancellationToken)
    {
        var user = new User(
            Guid.NewGuid().ToString(),
            request.Request.Email,
            request.Request.FullName.Trim(),
            passwordHasher.Hash(request.Request.Password),
            UserRoles.User);

        if (!await userCommands.TryCreateUser(user, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var (token, expiresAt) = tokenService.CreateToken(user);
        return new AuthResponse(user.Id, user.Email, user.FullName, token, expiresAt);
    }
}
