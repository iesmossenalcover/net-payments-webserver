using Application.Common;
using Application.Common.Services;
using Domain.Entities.Events;
using MediatR;

namespace Application.Events.Queries;

# region ViewModels
public record EventVm(
    long Id, string Code, string Description, string Name, decimal Price, decimal AmipaPrice,
    bool Enrollment, bool Amipa,
    DateTimeOffset CreationDate, DateTimeOffset PublishDate, DateTimeOffset? UnpublishDate, bool IsActive
);

#endregion

#region Query
public record GetEventByIdQuery(long Id) : IRequest<Response<EventVm>>;
#endregion

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, Response<EventVm>>
{
    #region  IOC
    private readonly IEventsRespository _eventsRepository;

    public GetEventByIdQueryHandler(IEventsRespository eventsRepository)
    {
        _eventsRepository = eventsRepository;
    }
    #endregion

    public async Task<Response<EventVm>> Handle(GetEventByIdQuery request, CancellationToken ct)
    {
        Event? e = await _eventsRepository.GetByIdAsync(request.Id, ct);
        if (e == null)
        {
            return Response<EventVm>.Error(ResponseCode.NotFound, "There is no event with this id");
        }

        return Response<EventVm>.Ok(
            new EventVm(
                e.Id, e.Code, e.Description, e.Name, e.Price, e.AmipaPrice, e.Enrollment, e.Amipa, e.CreationDate, e.PublishDate, e.UnpublishDate, e.IsActive
            ));
    }
}
