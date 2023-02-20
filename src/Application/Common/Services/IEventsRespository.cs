using Domain.Entities.Events;

namespace Application.Common.Services;

public interface IEventsRespository : IRepository<Event>
{
}