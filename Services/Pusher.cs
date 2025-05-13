using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Contracts;
using MvcMovie.Data;
using MvcMovie.Entities;

namespace MvcMovie.Services;

public class Pusher<T> : IPusher<T>
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public Pusher(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task PushAsync(T data, int type, CancellationToken cancellationToken = default)
    {
        switch (type)
        {
            case 1:
                await SaveToDatabase(data, cancellationToken);
                break;
            // case 2:
            //     await SaveToFile(data, cancellationToken);
            //     break;
            default:
                throw new NotSupportedException($"Push type {type} is not supported.");
        }
    }

    private async Task SaveRangeToDatabase(IEnumerable<T> data, CancellationToken cancellationToken)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        await context.AddRangeAsync(data, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SaveToDatabase(T data, CancellationToken cancellationToken)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        await context.AddAsync(data, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }


    public async Task PushManyAsync(IEnumerable<T> products, int type, CancellationToken cancellationToken)
    {
        switch (type)
        {
            case 1:
               await SaveRangeToDatabase(products, cancellationToken);
                break;

            default:
                throw new NotSupportedException($"PushManyAsync type {type} is not supported.");
        }
    }
    
}
