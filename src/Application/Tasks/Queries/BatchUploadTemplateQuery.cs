using Application.Common.Models;
using Domain.Services;
using Domain.Entities.People;
using MediatR;

namespace Application.GoogleWorkspace.Commands;

// Model we receive
public record BatchUploadTemplateQuery() : IRequest<FileVm>;

// Handler
public class BatchUploadTemplateQueryHandler : IRequestHandler<BatchUploadTemplateQuery, FileVm>
{
    #region props
    private readonly ICsvParser _csvParser;

    public BatchUploadTemplateQueryHandler(ICsvParser csvParser)
    {
        _csvParser = csvParser;
    }
    #endregion


    public async Task<FileVm> Handle(BatchUploadTemplateQuery request, CancellationToken ct)
    {
        var memStream = new MemoryStream();
        var streamWriter = new StreamWriter(memStream);
        await _csvParser.WriteToStreamAsync(streamWriter, new List<BatchUploadRow>());
        return new FileVm(memStream, "text/csv", "plantilla.csv");
    }
}
