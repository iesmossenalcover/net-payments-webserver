using System.Globalization;
using Application.Common.Models;
using Application.Common.Services;
using Application.GoogleWorkspace.Commands;
using CsvHelper;
using CsvHelper.Configuration;

namespace Infrastructure;

public class CsvParser : ICsvParser
{
    private static readonly Dictionary<Type, Type> Map = new Dictionary<Type, Type>()
    {
        { typeof(AccountRow), typeof(GoogleUserMap) },
        { typeof(BatchUploadRow), typeof(BatchUploadRowMap) },
        { typeof(WifiAccountRow), typeof(WifiAccountRowMap) },
    };

    public CsvParseResult<T> Parse<T>(Stream stream)
    {
        var result = new CsvParseResult<T>();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        try
        {
            AddMapIfExist(typeof(T), csv.Context);
            result.Values = csv.GetRecords<T>().ToList();
            result.Ok = true;
        }
        catch (CsvHelperException e)
        {
            CsvContext context = e.Context;
            result.Ok = false;
            string column = csv.CurrentIndex.ToString();
            if (csv.HeaderRecord != null && csv.CurrentIndex < csv.HeaderRecord.Length)
            {
                column = csv.HeaderRecord[csv.CurrentIndex];
            }
            result.ErrorMessage = $"Error columna: {column},  fila {context.Parser.Row}";
        }
        return result;
    }

    public async Task WriteManyToFileAsync<T>(string path, IEnumerable<T> records, bool overrite)
    {
        using var writer = new StreamWriter(path, !overrite);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        AddMapIfExist(typeof(T), csv.Context);
        if (overrite)
        {
            csv.WriteHeader<T>();
            csv.NextRecord();
        }
        foreach (var r in records)
        {
            csv.WriteRecord(r);
            csv.NextRecord();
        }
        await csv.FlushAsync();
    }

    public async Task WriteHeadersAsync<T>(string path)
    {
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        AddMapIfExist(typeof(T), csv.Context);

        csv.WriteHeader<T>();
        csv.NextRecord();
        await csv.FlushAsync();
    }

    public async Task WriteToFileAsync<T>(string path, T record, bool overrite)
    {
        using var writer = new StreamWriter(path, !overrite);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecord(record);
        csv.NextRecord();
        await csv.FlushAsync();
    }

    public async Task WriteToStreamAsync<T>(StreamWriter writer, IEnumerable<T> records)
    {
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture, true);
        AddMapIfExist(typeof(T), csv.Context);

        await csv.WriteRecordsAsync(records);
        await csv.FlushAsync();
    }

    private void AddMapIfExist(Type type, CsvContext context)
    {
        if (Map.TryGetValue(type, out Type? mapper))
        {
            context.RegisterClassMap(mapper);
        }
    }
}

public class BatchUploadRowMap : ClassMap<BatchUploadRow>
{
    public BatchUploadRowMap()
    {
        Map(m => m.Expedient);
        Map(m => m.Identitat).Validate(x => !string.IsNullOrEmpty(x.Field));
        Map(m => m.Nom).Validate(x => !string.IsNullOrEmpty(x.Field));
        Map(m => m.Llinatge1).Validate(x => !string.IsNullOrEmpty(x.Field));
        Map(m => m.Llinatge2);
        Map(m => m.TelContacte);
        Map(m => m.Grup).Validate(x => !string.IsNullOrEmpty(x.Field));
        Map(m => m.Assignatures);
    }
}

public class WifiAccountRowMap : ClassMap<WifiAccountRow>
{
    public WifiAccountRowMap()
    {
        Map(m => m.Email).Name("usuari");
        Map(m => m.Password).Name("password");
    }
}

// public class PersonRowMap : ClassMap<PersonRow>
// {
//     public WifiAccountRowMap()
//     {
//         Map(m => m.Email).Name("usuari");
//         Map(m => m.Password).Name("password");
//     }
// }

public class GoogleUserMap : ClassMap<AccountRow>
{
    public GoogleUserMap()
    {
        Map(m => m.First).Name("First Name [Required]");
        Map(m => m.Last).Name("Last Name [Required]");
        Map(m => m.Email).Name("Email Address [Required]");
        Map(m => m.Password).Name("Password [Required]");
        Map(m => m.PasswordHash).Name("Password Hash Function [UPLOAD ONLY]");
        Map(m => m.Org).Name("Org Unit Path [Required]");
        Map(m => m.New).Name("New Primary Email [UPLOAD ONLY]");
        Map(m => m.Recovery).Name("Recovery Email");
        Map(m => m.Home).Name("Home Secondary Email");
        Map(m => m.Work).Name("Work Secondary Email");
        Map(m => m.RecoveryPhone).Name("Recovery Phone [MUST BE IN THE E.164 FORMAT]");
        Map(m => m.WorkPhone).Name("Work Phone");
        Map(m => m.HomePhone).Name("Home Phone");
        Map(m => m.Mobile).Name("Mobile Phone");
        Map(m => m.WorkAddress).Name("Work Address");
        Map(m => m.HomeAddress).Name("Home Address");
        Map(m => m.Employee).Name("Employee ID");
        Map(m => m.EmployeeType).Name("Employee Type");
        Map(m => m.EmployeeTitle).Name("Employee Title");
        Map(m => m.Manager).Name("Manager Email");
        Map(m => m.Department).Name("Department");
        Map(m => m.Cost).Name("Cost Center");
        Map(m => m.Building).Name("Building ID");
        Map(m => m.Floor).Name("Floor Name");
        Map(m => m.FloorSection).Name("Floor Section");
        Map(m => m.Change).Name("Change Password at Next Sign-In");
        Map(m => m.NewStatus).Name("New Status [UPLOAD ONLY]");
        Map(m => m.Advanced).Name("Advanced Protection Program enrollment");
    }
}