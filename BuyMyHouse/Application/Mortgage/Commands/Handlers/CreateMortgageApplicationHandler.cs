using BuyMyHouse.Application.Common.Mappings;
using BuyMyHouse.Application.Common.Services;
using MediatR;

namespace BuyMyHouse.Application.Mortgage.Commands.Handlers;

public class CreateMortgageApplicationHandler(IMortgageApplicationCommands applicationCommands, IMortgageApplicationQueue applicationQueue)
    : IRequestHandler<CreateMortgageApplication, Guid>
{
    public async Task<Guid> Handle(CreateMortgageApplication request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var application = request.Request.ToEntity(request.UserId, id.ToString());

        // Store first, so the processor never receives a row key that doesn't exist yet.
        await applicationCommands.CreateMortgageApplication(application, cancellationToken);

        try
        {
            await applicationQueue.Enqueue(application.RowKey, cancellationToken);
        }
        catch
        {
            // An application that never reaches the queue would never be processed; roll it back so the user can retry.
            await applicationCommands.DeleteMortgageApplication(application, CancellationToken.None);
            throw;
        }

        return id;
    }
}
