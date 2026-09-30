using Application.Common;
using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.Groups.Commands;

public record DeleteGroupCommand(long Id) : IRequest<Response<long?>>;
public class DeleteGroupCommandHandler : IRequestHandler<DeleteGroupCommand, Response<long?>>
{
    #region IOC
    private readonly IGroupsRepository _groupsRepo;
    private readonly IPersonGroupCourseRepository _personGroupCourseRepo;

    public DeleteGroupCommandHandler(IGroupsRepository groupsRepo, IPersonGroupCourseRepository personGroupCourseRepo)
    {
        _groupsRepo = groupsRepo;
        _personGroupCourseRepo = personGroupCourseRepo;
    }
    #endregion

    public async Task<Response<long?>> Handle(DeleteGroupCommand request, CancellationToken ct)
    {
        Group? group = await _groupsRepo.GetByIdAsync(request.Id, ct);

        if (group == null) return Response<long?>.Error(ResponseCode.BadRequest, "El grup no existeix.");

        if (await _personGroupCourseRepo.AnyByGroupIdAsync(group.Id, ct))
        {
            return Response<long?>.Error(ResponseCode.BadRequest, "No es pot eliminar el grup perquè té persones assignades.");
        }

        try
        {
            await _groupsRepo.DeleteAsync(group, CancellationToken.None);
        }
        catch (Exception)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, "No es pot eliminar el grup.");
        }

        return Response<long?>.Ok(group.Id);
    }
}
