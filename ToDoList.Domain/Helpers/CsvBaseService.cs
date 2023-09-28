using System.Collections;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace ToDoList.Domain.Helpers;

public class CsvBaseService<T>
{
    private readonly CsvConfiguration _csvConfiguration;
    
    public CsvBaseService()
    {
        _csvConfiguration = GetConfiguration();
    }

    public byte[] UploadFile(IEnumerable data)
    {
        using (var memoryStream = new MemoryStream())
        using (var streamWrite = new StreamWriter(memoryStream))
        using (var csvWriter = new CsvWriter(streamWrite, _csvConfiguration))
        {
            csvWriter.WriteRecords(data);
            streamWrite.Flush();
            return memoryStream.ToArray();
        }
    }

    private CsvConfiguration GetConfiguration()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            Encoding = Encoding.UTF8,
            NewLine = "\r\n"
        };
    }
}