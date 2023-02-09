using System.Data;
using System.Globalization;
using Application.Common.Models;
using Application.Common.Services;
using CsvHelper;
using CsvHelper.Configuration;

namespace Infrastructure;

public class CsvParser : ICsvParser
{
    public CsvParserResult<BatchUploadRowModel> ParseBatchUpload(Stream stream)
    {
        var result = new CsvParserResult<BatchUploadRowModel>();
        try
        {
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<BatchUploadRowModelMap>();
            result.Values = csv.GetRecords<BatchUploadRowModel>().ToList();
            result.Ok = true;
        }
        catch (CsvHelperException e)
        {
            result.Ok = false;
            result.ErrorMessage = e.Message;
        }
        return result;
    }
}

public class BatchUploadRowModelMap : ClassMap<BatchUploadRowModel>
{
    public BatchUploadRowModelMap()
    {
        Map(m => m.Expedient);
        Map(m => m.Identitat).Validate(x => !string.IsNullOrEmpty(x.Field));
        Map(m => m.Nom).Validate(x => !string.IsNullOrEmpty(x.Field));
        Map(m => m.Llinatge1).Validate(x => !string.IsNullOrEmpty(x.Field));
        Map(m => m.Prematricula).Validate(x => x.Field == "1" || x.Field == "0");
        Map(m => m.Pagament).Validate(x => x.Field == "1" || x.Field == "0");
        Map(m => m.Pagament).Validate(x => !string.IsNullOrEmpty(x.Field));
    }
}