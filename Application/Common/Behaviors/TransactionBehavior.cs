using MediatR;
using TeamAvailability.Application.Common.Interfaces;

namespace TeamAvailability.Application.Common.Behaviors;

public interface ITransactionalRequest;

public sealed class TransactionBehavior<TRequest, TResponse>(IApplicationDbContext db)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, ITransactionalRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (db is not Microsoft.EntityFrameworkCore.DbContext context)
            return await next();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var response = await next();
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
