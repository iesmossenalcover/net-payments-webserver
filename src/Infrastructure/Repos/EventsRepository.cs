using Domain.Entities.Events;

namespace Infrastructure.Repos;

public class EventsRepository : Repository<Event>, Application.Common.Services.IEventsRespository
{
    public EventsRepository(AppDbContext dbContext) : base(dbContext, dbContext.Events) {}
}