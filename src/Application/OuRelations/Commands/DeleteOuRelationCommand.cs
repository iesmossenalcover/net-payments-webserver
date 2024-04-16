using Application.Common;
using Domain.Services;
using Domain.Entities.Events;
using MediatR;
using Domain.Entities.GoogleApi;

namespace Application.OuRelations.Commands;

public record DeleteOuRelationCommand(long Id) : IRequest<Response<long?>>;
public class DeleteOuRelationCommandHandler : IRequestHandler<DeleteOuRelationCommand, Response<long?>>
{
    #region IOC
    private readonly IOUGroupRelationsRepository _groupsRelationRepo;

    public DeleteOuRelationCommandHandler(IOUGroupRelationsRepository groupsRelationRepo)
    {
        _groupsRelationRepo = groupsRelationRepo;
    }
    #endregion

    public async Task<Response<long?>> Handle(DeleteOuRelationCommand request, CancellationToken ct)
    {
        OuGroupRelation? relation = await _groupsRelationRepo.GetByIdAsync(request.Id, ct);

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