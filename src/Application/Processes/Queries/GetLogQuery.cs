using Application.Common;
using Domain.Entities.Logs;
using Domain.Services;
using Domain.ValueObjects;
using MediatR;

namespace Application.Processes.Queries;

# region ViewModels
public record GetLogQueryVm(string Data);
#endregion

public record GetLogQuery(long Id) : IRequest<Response<GetLogQueryVm>>;

public class GetLogQueryHandler : IRequestHandler<GetLogQuery, Response<GetLogQueryVm>>
{
    # region IOC
    private readonly ILogsInfoRespository _logsInfoRespository;
    private readonly ILogStore _logStore;

    public GetLogQueryHandler(ILogsInfoRespository logsInfoRespository, ILogStore logStore)
    {
        _logsInfoRespository = logsInfoRespository;
        _logStore = logStore;
    }
    #endregion

    public async Task<Response<GetLogQueryVm>> Handle(GetLogQuery request, CancellationToken ct)
    {
        LogStoreInfo? logStoreInfo = await _logsInfoRespository.GetByIdAsync(request.Id, ct);
        if (logStoreInfo == null) return Response<GetLogQueryVm>.Error(ResponseCode.BadRequest, "Log no trobat");

        Log? log = await _logStore.Read(logStoreInfo);
        if (log == null) return Response<GetLogQueryVm>.Error(ResponseCode.BadRequest, "Log no trobat");

        return Response<GetLogQueryVm>.Ok(new GetLogQueryVm(log.Data));
    }
}
