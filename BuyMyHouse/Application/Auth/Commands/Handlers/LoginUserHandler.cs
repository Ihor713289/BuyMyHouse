using BuyMyHouse.Application.Common.DTOs;
using BuyMyHouse.Application.Common.Exceptions;
using BuyMyHouse.Application.Common.Services;
using MediatR;

namespace BuyMyHouse.Application.Auth.Commands.Handlers;

public class LoginUserHandler(IUserQueries userQueries, IPasswordHasher passwordHasher, ITokenService tokenService)
    : IRequestHandler<LoginUser, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginUser request, CancellationToken cancellationToken)
    {
        var user = await userQueries.GetUserByEmail(request.Request.Email, cancellationToken);

        // Same error for unknown email and wrong password, so accounts cannot be enumerated.
        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Request.Password))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var (token, expiresAt) = tokenService.CreateToken(user);
        return new AuthResponse(user.Id, user.Email, user.FullName, token, expiresAt);
    }
}
