using DevKit.Application.Ports;
using DevKit.Domain.Events;
using DevKit.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevKit.Infrastructure.EfCore;

public static class DbContextExtensions
{
    public static DbContext ApplyAudit(this DbContext context, ICurrentUserService currentUserService,
        IClock clock) {
        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>()) {
#pragma warning disable IDE0010 // Add missing cases: using default handler
            switch (entry.State)
#pragma warning restore IDE0010
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = currentUserService.UserId;
                    entry.Entity.CreatedAt = clock.Now;
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = currentUserService.UserId;
                    entry.Entity.LastModifiedAt = clock.Now;
                    break;
                default: continue;
            }
        }

        return context;
    }

    public static DbContext ApplySoftDelete(this DbContext context) {
        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>()
                     .Where(entry => entry.State == EntityState.Deleted)) {
            entry.Entity.IsDeleted = true;
            entry.State = EntityState.Modified;
        }

        return context;
    }

    public static async Task<DbContext> DispatchEventsAsync(this DbContext context,
        IDomainEventDispatcher eventDispatcher, ILogger logger, CancellationToken cancellationToken) {
        logger.LogDebug("Start dispatching events");
        while (true) {
            var allDomainEvents = context.ChangeTracker.Entries<IHasDomainEvent>()
                .Select(x => x.Entity.DomainEvents)
                .SelectMany(x => x);
            var domainEventEntity = allDomainEvents.FirstOrDefault(domainEvent => !domainEvent.IsPublished);
            if (domainEventEntity == null) break;

            domainEventEntity.IsPublished = true;
            await eventDispatcher.Publish(domainEventEntity, cancellationToken);
        }

        return context;
    }
}
