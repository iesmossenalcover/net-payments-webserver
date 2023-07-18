using System.Globalization;
using Application.Common.Models;
using Domain.Services;
using Application.GoogleWorkspace.Commands;
using CsvHelper;
using CsvHelper.Configuration;

namespace Infrastructure;

public class CsvParser : ICsvParser
{
    public static string[] TRUE_VALUES = new string[] { "si", "sí", "SI", "Sí", "Sí", "Si", "S", "s" };
    public static string[] FALSE_VALUES = new string[] { "no", "NO", "No", "nO", "N" };

    private static readonly Dictionary<Type, Type> Map = new Dictionary<Type, Type>()
    {
        { typeof(AccountRow), typeof(GoogleUserMap) },
        { typeof(BatchUploadRow), typeof(BatchUploadRowMap) },
        { typeof(WifiAccountRow), typeof(WifiAccountRowMap) },
        { typeof(PersonRow), typeof(PersonRowMap) },
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
            if (csv.CurrentIndex < 0)
            {
                result.ErrorMessage = $"Error: {e.Message}";
            }
            else if (csv.HeaderRecord != null && csv.CurrentIndex < csv.HeaderRecord.Length)
            {
                column = csv.HeaderRecord[csv.CurrentIndex];
                result.ErrorMessage = $"Error columna: {column},  fila {context.Parser.Row}";
            }
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
        Map(m => m.AcademicRecordNumber).Name("Expedient");
        Map(m => m.DocumentId).Name("Identitat").Validate(x => !string.IsNullOrEmpty(x.Field));
        Map(m => m.FirstName).Name("Nom").Validate(x => !string.IsNullOrEmpty(x.Field));
        Map(m => m.Surname1).Name("Llinatge1").Validate(x => !string.IsNullOrEmpty(x.Field));
        Map(m => m.Surname2).Name("Llinatge2");
        Map(m => m.ContactPhone).Name("TelContacte");
        Map(m => m.GroupName).Name("Grup");
        Map(m => m.Subjects).Name("Assignatures");
        Map(m => m.Email).Name("Correu");
        Map(m => m.IsAmipa).Name("Amipa")
            .TypeConverterOption.BooleanValues(true, true, CsvParser.TRUE_VALUES)
            .TypeConverterOption.BooleanValues(false, true, CsvParser.FALSE_VALUES);

        Map(m => m.Enrolled).Name("Matriculat")
            .TypeConverterOption.BooleanValues(true, true, CsvParser.TRUE_VALUES)
            .TypeConverterOption.BooleanValues(false, true, CsvParser.FALSE_VALUES);
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

public class PersonRowMap : ClassMap<Application.Common.Models.PersonRow>
{
    public PersonRowMap()
    {
        Map(m => m.Name).Name("Nom");
        Map(m => m.Surname1).Name("Llinatge1");
        Map(m => m.Surname2).Name("Llinatge2");
        Map(m => m.DocumentId).Name("Document Identitat");
        Map(m => m.AcademicRecordNumber).Name("Expedient academic");
        Map(m => m.GroupName).Name("Grup");
        Map(m => m.Email).Name("Correu");
        Map(m => m.Amipa).Name("Amipa");
        Map(m => m.Enrolled).Name("Matriculat");
    }
}

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