using Application.Common;
using Domain.Services;
using Domain.Entities.Events;
using MediatR;
using Domain.Entities.GoogleApi;

namespace Application.OURelations.Commands;

public record DeleteOURelationCommand(long Id) : IRequest<Response<long?>>;
public class DeleteOURelationCommandHandler : IRequestHandler<DeleteOURelationCommand, Response<long?>>
{
    #region IOC
    private readonly IOUGroupRelationsRepository _groupsRelationRepo;

    public DeleteOURelationCommandHandler(IOUGroupRelationsRepository groupsRelationRepo)
    {
        _groupsRelationRepo = groupsRelationRepo;
    }
    #endregion

    public async Task<Response<long?>> Handle(DeleteOURelationCommand request, CancellationToken ct)
    {
        UoGroupRelation? relation = await _groupsRelationRepo.GetByIdAsync(request.Id, ct);

        if (relation == null) return Response<long?>.Error(ResponseCode.BadRequest, "OU relation no existeix");

        try
        {
            await _groupsRelationRepo.DeleteAsync(relation, CancellationToken.None);
        }
        catch (Exception)
        {
            return Response<long?>.Error(ResponseCode.BadRequest, "No es pot eliminar l'OU relation.");
        }

        return Response<long?>.Ok(relation.Id);
    }


}