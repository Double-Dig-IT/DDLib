
namespace DDigit.ExternalDataSources;

public class ResolveExternalResources
{
  public static async Task<ExternalResource[]> ResolveAsync(Record record, SqlStateInfo sqlState)
  {
    ExternalResource GetExternalResourceFunc(FieldData field,
                                             string tempFile,
                                             string fieldValue)
    {
      return field.MediaStorageType switch
      {
        MediaStorageTypeEnum.UploadUrl
          => RemoteExternalResource(record, field, tempFile, fieldValue, sqlState.CancellationToken),

        MediaStorageTypeEnum.FileSystem
          => LocalExternalResource(record, field, tempFile, fieldValue, sqlState.CancellationToken),

        _ => throw new NotImplementedException($"{nameof(field.MediaStorageType)}: {field.MediaStorageType}")
      };
    }


    var result = new List<ExternalResource>();
    var tempFolder = Path.GetTempPath();

    // Retrieve upload fields.
    var uploadFields = record.Database!.Fields.Where(f => f.Type == FieldTypeEnum.Application ||
                                                          f.Type == FieldTypeEnum.Image);

    foreach (var field in uploadFields)
    {
      if (field.IsLinked)
      {
        // Linked Fields should get resolved in the linked record in different step
        continue;
      }

      var repCount = record.RepCount(field);
      for (var occ = 1; occ <= repCount; occ++)
      {
        var tempFile = await record.GetAsync(field.Name!, occ, sqlState);
        if (tempFile is not null && tempFile.StartsWith(tempFolder, StringComparison.OrdinalIgnoreCase))
        {
          if (!File.Exists(tempFile))
          {
            throw new DDException($"Temp image file '{tempFile}' could not be found");
          }
          if (string.IsNullOrEmpty(field.FormatString))
          {
            throw new DDException($"Field '{field}' from '{record!.Database!.Name}' does not contain an image path");
          }

          // TODO: Field value should optionally be changed to guid - but right now this is done with a dirty fix...
          var fieldValue = field.AutoNumberAssignment == AutoNumberAssignmentEnum.OnDigitalAsset && field.AutoNumberFormatString == "{guid}" ?
            Guid.NewGuid().ToString() : Path.GetFileName(tempFile);

          // Store the file, either local on remote based on the field configuration.
          result.Add(GetExternalResourceFunc(field, tempFile, fieldValue));

          // Set the field value
          record.Set(field.Name!, occ, fieldValue);

          // Set the original file name
          if (field.OriginalFileNameTag is not null)
          {
            record.Set(field.OriginalFileNameTag, occ, Path.GetFileName(tempFile));
          }
        }
      }
    }

    return [.. result];
  }

  public record ExternalResource(Func<Task> ResolveAsync);


  private static ExternalResource LocalExternalResource(Record record,
                                                        FieldData field,
                                                        string tempFile,
                                                        string fieldValue,
                                                        CancellationToken cancellationToken)
  {
    return new(() => StoreLocallyAsync(record, field, tempFile, fieldValue, cancellationToken));
  }

  private static ExternalResource RemoteExternalResource(Record record,
                                                         FieldData field,
                                                         string tempFile,
                                                         string fieldValue,
                                                         CancellationToken cancellationToken)
  {
    if (field.FormatString.Contains("datamachine")) // TODO: Dirty check...
    {
      return new(() => UploadCouranteAsync(record, field, tempFile, fieldValue, cancellationToken));
    }
    throw new NotImplementedException($"{nameof(field.MediaStorageType)}: {field.MediaStorageType}");
  }


  private static async Task StoreLocallyAsync(Record record,
                                              FieldData field,
                                              string tempFile,
                                              string fieldValue,
                                              CancellationToken cancellationToken)
  {
    try
    {
      var directory = Path.Combine(record!.Database!.Folder ?? "", field.RetrievalPath.Replace("%data%", ""));
      Directory.CreateDirectory(directory);

      var destination = Path.Combine(directory, fieldValue);
      if (File.Exists(destination))
      {
        throw new DDException($"Image with name '{fieldValue}' is already present in the destination folder");
      }
      else
      {
        await using var source = ReadFileStream(tempFile);
        await using var dest = WriteFileStream(destination);

        await source.CopyToAsync(dest, cancellationToken);
      }
    }
    finally
    {
      // Delete original temporary file
      File.Delete(tempFile);
    }
  }



  private static async Task UploadCouranteAsync(Record record,
                                                FieldData field,
                                                string tempFile,
                                                string fieldValue,
                                                CancellationToken cancellationToken)
  {
    try
    {
      if (!Uri.TryCreate(field.FormatString, UriKind.Absolute, out var uri))
      {
        throw new DDException($"Storage path not in correct uri format '{field.FormatString}'");
      }
      if (field.AutoNumberSuffix is not { } key)
      {
        throw new DDException($"Courante Api key not found in correct spot '{nameof(field.AutoNumberSuffix)}'");
      }
      if (!Guid.TryParse(fieldValue, out _))
      {
        throw new DDException($"Courante Api requires media reference value to be GUID: '{fieldValue}'");
      }

      using var httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Add("apikey", key);

      await using var source = ReadFileStream(tempFile);
      using var content = new MultipartFormDataContent()
        {
          { new StringContent(fieldValue), "uuid" },
          { new StringContent($"{record.Id}"), "priref" },
          { new StreamContent(source), "file", fieldValue }
        };

      var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
      var response = await httpClient.SendAsync(request,
        HttpCompletionOption.ResponseContentRead, cancellationToken);

      response.EnsureSuccessStatusCode();

      // Handy for debugging...
      var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
      ;
    }
    finally
    {
      // Delete original temporary file
      File.Delete(tempFile);
    }
  }

  private static FileStream ReadFileStream(string path) => new(path, new FileStreamOptions
  {
    Access = FileAccess.Read,
    Mode = FileMode.Open,
    Options = FileOptions.Asynchronous
  });

  private static FileStream WriteFileStream(string path) => new(path, new FileStreamOptions
  {
    Access = FileAccess.Write,
    Mode = FileMode.Create,
    Options = FileOptions.Asynchronous
  });
}
