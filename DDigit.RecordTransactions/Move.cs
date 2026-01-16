using System.Net.NetworkInformation;

namespace DDigit.RecordTransactions;

public class Move : DatabaseAccess
{
  public static async Task<Result<int>> SearchObjectAsync(string folder,
                                                          string objectDatabase,
                                                          string barcodeObjectNumberOrId,
                                                          CancellationToken cancellationToken)

  {
    var provider = new DDataProvider(new MSSqlRepository());
    var database = OpenDatabase(folder, objectDatabase);

    var objectId = await provider.SearchObjectAsync(database, barcodeObjectNumberOrId, cancellationToken);
    if (objectId is not int id)
    {
      return Result<int>.CreateErrorResult(MessageSeverityEnum.Info, ErrorCodeEnum.ObjectRecordNotFound, barcodeObjectNumberOrId);
    }

    return Result<int>.CreateSuccessResult(id);
  }

  public static async Task<Result<Record>> GetObjectAsync(string folder,
                                                          string objectDatabase,
                                                          int objectId,
                                                          CancellationToken cancellationToken)

  {
    var provider = new DDataProvider(new MSSqlRepository());
    var database = OpenDatabase(folder, objectDatabase);

    var objectRecord = await provider.ReadRecordAsync(database, objectId, cancellationToken);
    if (objectRecord is not Record record)
    {
      return Result<Record>.CreateErrorResult(MessageSeverityEnum.Info, ErrorCodeEnum.ObjectRecordNotFound, objectId);
    }

    return Result<Record>.CreateSuccessResult(record);
  }

  public static async Task<Result<int>> SearchLocationAsync(string folder,
                                                            string locationDatabase,
                                                            string barcodeLocationNameOrId,
                                                            CancellationToken cancellationToken)
  {
    var provider = new DDataProvider(new MSSqlRepository());
    var database = OpenDatabase(folder, locationDatabase);
    var locationId = await provider.SearchLocationAsync(database, barcodeLocationNameOrId, cancellationToken);
    if (locationId is not int id)
    {
      return Result<int>.CreateErrorResult(MessageSeverityEnum.Info, ErrorCodeEnum.LocationRecordNotFound, barcodeLocationNameOrId);
    }

    return Result<int>.CreateSuccessResult(id);
  }

  public static async Task<Result<Record>> GetLocationAsync(string folder,
                                                            string locationDatabase,
                                                            int locationId,
                                                            CancellationToken cancellationToken)
  {
    var provider = new DDataProvider(new MSSqlRepository());
    var database = OpenDatabase(folder, locationDatabase);
    var locationRecord = await provider.ReadRecordAsync(database, locationId, cancellationToken);
    if (locationRecord is not Record record)
    {
      return Result<Record>.CreateErrorResult(MessageSeverityEnum.Info, ErrorCodeEnum.LocationRecordNotFound, locationId);
    }

    return Result<Record>.CreateSuccessResult(record);
  }

  public static async Task<Result<Record>> MoveObject(string folder,
                                                      string objectDatabaseName,
                                                      int objectId,
                                                      string locationDatabaseName,
                                                      int locationId,
                                                      string? executor = null,
                                                      string? notes = null)
  {
    var objectsDatabase = OpenDatabase(folder, objectDatabaseName);
    var locationsDatabase = OpenDatabase(folder, locationDatabaseName);

    if (!objectsDatabase.SupportsMove)
    {
      return Result<Record>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.MoveIsNotSupported, objectDatabaseName);
    }

    var locationFields = objectsDatabase.LocationFields!;

    var dataProvider = new DDataProvider(new MSSqlRepository());
    if (await dataProvider.ReadRecordAsync(objectsDatabase, objectId) is not Record objectRecord)
    {
      return Result<Record>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.ObjectRecordNotFound, objectId);
    }

    if (await dataProvider.ReadRecordAsync(locationsDatabase, locationId) is null)
    {
      return Result<Record>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.LocationRecordNotFound, locationId);
    }

    var oldLocation = LocationData.GetRow(objectRecord, locationFields);
    if (oldLocation.Id == 0 && locationsDatabase.SupportsHomeLocation) // this is to default to an objects home location if the current location is not set
    {
      oldLocation = LocationData.GetRow(objectRecord, objectsDatabase.HomeLocationFields!);
    }

    if (oldLocation.Id == locationId)
    {
      return Result<Record>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.ObjectIsAlreadyAtThisLocation, locationId);
    }

    var now = DateTime.Now;
    if (objectsDatabase.SupportsMoveHistory)
    {
      var locationHistoryFields = objectsDatabase.LocationHistoryFields!;
      var lastOcc = objectRecord.RepCount(locationHistoryFields.Id!);

      for (int occ = lastOcc; occ > 0; occ--)
      {
        var locationHistoryRow = LocationData.GetRow(objectRecord, locationHistoryFields, occ);
        LocationData.SetRow(objectRecord, locationHistoryFields, locationHistoryRow, occ + 1);
      }

      if (oldLocation.Id != 0)
      {
        oldLocation.EndDate = DateOnly.FromDateTime(now);
        oldLocation.EndTime = TimeOnly.FromDateTime(now);
        LocationData.SetRow(objectRecord, locationHistoryFields, oldLocation);
      }
    }

    var newLocation = new LocationData
    {
      Id = locationId,
      StartDate = DateOnly.FromDateTime(now),
      StartTime = TimeOnly.FromDateTime(now),
      Executor = executor,
      Notes = notes
    };

    LocationData.SetRow(objectRecord, locationFields, newLocation);
    await objectRecord.WriteAsync(new SqlStateInfo());
    return Result<Record>.CreateSuccessResult(objectRecord);
  }

  public static async Task<Result<Record>> MovePackage(string folder,
                                                       string locationDatabaseName,
                                                       int packageId,
                                                       int locationId,
                                                       string? executor = null,
                                                       string? notes = null)
  {
    var locationsDatabase = OpenDatabase(folder, locationDatabaseName);

    if (!locationsDatabase.SupportsMove)
    {
      return Result<Record>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.MoveIsNotSupported, locationDatabaseName);
    }

    var locationFields = locationsDatabase.LocationFields!;

    var dataProvider = new DDataProvider(new MSSqlRepository());

    if (await dataProvider.ReadRecordAsync(locationsDatabase, packageId) is not Record packageRecord)
    {
      return Result<Record>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.LocationRecordNotFound, packageId);
    }

    // only PACKAGE records can be moved
    if (packageRecord["package_location"] != "PACKAGE")
    {
      return Result<Record>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.CannotMoveLocation, packageId);
    }

    if (await dataProvider.ReadRecordAsync(locationsDatabase, locationId) is null)
    {
      return Result<Record>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.LocationRecordNotFound, locationId);
    }

    var oldLocation = LocationData.GetRow(packageRecord, locationFields);
    if (oldLocation.Id == 0) // this is to default to a packages home location if the current location is not set
    {
      oldLocation = LocationData.GetRow(packageRecord, locationsDatabase.HomeLocationFields!);
    }

    if (oldLocation.Id == locationId)
    {
      return Result<Record>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.PackageIsAlreadyAtThisLocation, locationId);
    }

    var now = DateTime.Now;
    if (locationsDatabase.SupportsMoveHistory)
    {
      var locationHistoryFields = locationsDatabase.LocationHistoryFields!;
      var lastOcc = packageRecord.RepCount(locationHistoryFields.Id!);

      for (int occ = lastOcc; occ > 0; occ--)
      {
        var locationHistoryRow = LocationData.GetRow(packageRecord, locationHistoryFields, occ);
        LocationData.SetRow(packageRecord, locationHistoryFields, locationHistoryRow, occ + 1);
      }

      if (oldLocation.Id != 0)
      {
        oldLocation.EndDate = DateOnly.FromDateTime(now);
        oldLocation.EndTime = TimeOnly.FromDateTime(now);
        LocationData.SetRow(packageRecord, locationHistoryFields, oldLocation);
      }
    }

    var newLocation = new LocationData
    {
      Id = locationId,
      StartDate = DateOnly.FromDateTime(now),
      StartTime = TimeOnly.FromDateTime(now),
      Executor = executor,
      Notes = notes
    };

    LocationData.SetRow(packageRecord, locationFields, newLocation);
    await packageRecord.WriteAsync(new SqlStateInfo());
    return Result<Record>.CreateSuccessResult(packageRecord);
  }

  public static async Task<Result<LocationData>> GetHomeLocationAsync(string folder,
                                                                      string databaseName,
                                                                      int recordId,
                                                                      CancellationToken cancellationToken)
  {
    var database = OpenDatabase(folder, databaseName);
    if (!database.SupportsHomeLocation)
    {
      return Result<LocationData>.CreateErrorResult(MessageSeverityEnum.Warning, ErrorCodeEnum.HomeLocationNotSupported, databaseName);
    }

    var dataProvider = new DDataProvider(new MSSqlRepository());
    var record = (await dataProvider.ReadRecordAsync(database, recordId, cancellationToken))!;
    // this method can be used for requesting the home location of both objects and packages (packages in model application 5.2)
    // so the following check is meant for when were working with packages
    if (database.FindFieldByTagOrName("package_location") is { } fieldData)
    {
      // only packages have a home location
      if (record[fieldData] != "PACKAGE")
      {
        return Result<LocationData>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.HomeLocationNotSupported, recordId);
      }
    }

    var homeLocation = LocationData.GetRow(record, database.HomeLocationFields!);
    if (homeLocation.Id <= 0)
    {
      return Result<LocationData>.CreateErrorResult(MessageSeverityEnum.Warning, ErrorCodeEnum.HomeLocationNotFound, recordId);
    }

    return Result<LocationData>.CreateSuccessResult(homeLocation);
  }

  public static async Task<Result<LocationData>> GetCurrentLocation(string folder,
                                                                    string databaseName,
                                                                    int recordId,
                                                                    CancellationToken cancellationToken)
  {
    var database = OpenDatabase(folder, databaseName);
    if (!database.SupportsMoveHistory)
    {
      return Result<LocationData>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.MoveHistoryNotSupported, databaseName);
    }

    var dataProvider = new DDataProvider(new MSSqlRepository());
    var record = (await dataProvider.ReadRecordAsync(database, recordId, cancellationToken))!;
    // this method can be used for requesting the current location of both objects and packages
    // so the following check is meant for when were working with packages
    if (database.FindFieldByTagOrName("package_location") is { } fieldData)
    {
      // only packages have a current location
      if (record[fieldData] != "PACKAGE")
      {
        return Result<LocationData>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.MoveHistoryNotSupported, recordId);
      }
    }

    var currentLocation = LocationData.GetRow(record, database.LocationFields!);
    if (currentLocation.Id <= 0)
    {
      return Result<LocationData>.CreateErrorResult(MessageSeverityEnum.Warning, ErrorCodeEnum.CurrentLocationNotFound, currentLocation.Id);
    }

    return Result<LocationData>.CreateSuccessResult(currentLocation);
  }

  public static async Task<Result<List<LocationData>>> GetLocationHistory(string folder,
                                                                          string databaseName,
                                                                          int recordId,
                                                                          CancellationToken cancellationToken)
  {
    var database = OpenDatabase(folder, databaseName);
    if (!database.SupportsMoveHistory)
    {
      return Result<List<LocationData>>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.MoveHistoryNotSupported, databaseName);
    }

    var dataProvider = new DDataProvider(new MSSqlRepository());
    var record = (await dataProvider.ReadRecordAsync(database, recordId, cancellationToken))!;
    // this method can be used for requesting the location history of both objects and packages
    // so the following check is meant for when were working with packages
    if (database.FindFieldByTagOrName("package_location") is { } fieldData)
    {
      // only packages have a move history
      if (record[fieldData] != "PACKAGE")
      {
        return Result<List<LocationData>>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.MoveHistoryNotSupported, recordId);
      }
    }

    var locationHistoryFieldData = database.LocationHistoryFields!;
    var occurences = record.RepCount(locationHistoryFieldData.Id!);

    var locationHistory = new List<LocationData>();
    for (int occ = 1; occ <= occurences; occ++)
    {
      var row = LocationData.GetRow(record, locationHistoryFieldData, occ);
      locationHistory.Add(row);
    }

    return Result<List<LocationData>>.CreateSuccessResult(locationHistory);
  }

  public static async Task<Result<List<ConditionData>>> GetConditions(string folder,
                                                                      string databaseName,
                                                                      int recordId,
                                                                      CancellationToken cancellationToken)
  {
    var database = OpenDatabase(folder, databaseName);
    if (!database.SupportsConditions)
    {
      return Result<List<ConditionData>>.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.ConditionIsNotSupported, databaseName);
    }

    var dataProvider = new DDataProvider(new MSSqlRepository());
    var record = (await dataProvider.ReadRecordAsync(database, recordId, cancellationToken))!;

    var conditionFieldData = database.ConditionFields!;
    var occurences = record.RepCount(conditionFieldData.Id!);

    var conditions = new List<ConditionData>();
    for (int occ = 1; occ <= occurences; occ++)
    {
      var row = ConditionData.GetRow(record, conditionFieldData, occ);
      conditions.Add(row);
    }

    return Result<List<ConditionData>>.CreateSuccessResult(conditions);
  }

  public static async Task<Result<List<int>>> GetLinkedObjects(string folder,
                                                               string objectDatabaseName,
                                                               int locationId,
                                                               CancellationToken cancellationToken)
  {
    string field = "current_location.lref";
    string statement = $"{field} = {locationId} startfrom 1 limit 1000";
    var database = OpenDatabase(folder, objectDatabaseName);
    var dataProvider = new DDataProvider(new MSSqlRepository());

    // SupportsMove is a weird name in this case but it checks if currentlocation.lref exists in the database
    if (!database.SupportsMove)
    {
      return (Result<List<int>>)Result.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.FieldNotFound, field);
    }

    var resultSet = await dataProvider.SearchAsync(folder, objectDatabaseName, null, statement, cancellationToken: cancellationToken);

    return Result<List<int>>.CreateSuccessResult(resultSet?.Ids??[]);
  }

  public static async Task<Result<List<int>>> GetLinkedLocations(string folder,
                                                                string locationDatabaseName,
                                                                int locationId,
                                                                CancellationToken cancellationToken,
                                                                string language = null)
  {
    var field = "part.lref";
    var locationResult = await GetLocationAsync(folder, "location", locationId, cancellationToken);
    var record = locationResult.Data!;

    if (record.Database!.FindFieldByTagOrName(field) == null)
    {
      return Result<List<int>>.CreateErrorResult(MessageSeverityEnum.Warning, ErrorCodeEnum.LinkedLocationsNotFound, field);
    }

    var result = new List<int>();

    var repCount = record.RepCount(field);
    for (int occ = 1; occ <= repCount; occ++)
    {
      if (record[field, occ, language ?? ""] is string linkedLocationIdStr 
          && int.TryParse(linkedLocationIdStr, out int linkedLocationId))
      {
        result.Add(linkedLocationId);
      }
    }

    return Result<List<int>>.CreateSuccessResult(result);
  }

  public static async Task<Result<List<string>>> GetAvailableConditionTerms(string folder, string databaseName)
  {
    var database = OpenDatabase(folder, databaseName);

    var conditionFields = database.ConditionFields!;

    var fieldName = conditionFields.Condition!.Name!;
    var links = await Links.SearchLinks(folder, databaseName, fieldName, "");
    var terms = new List<string>();
    foreach (var link in links)
    {
      var value = link["term"];
      if (value is not null)
      {
        terms.Add(value);
      }
    }

    return Result<List<string>>.CreateSuccessResult(terms);
  }

  public static async Task<List<string>> GetNotes(string folder,
                                                  string databaseName,
                                                  int recordId,
                                                  CancellationToken cancellationToken)
  {
    var database = OpenDatabase(folder, databaseName);
    var dataProvider = new DDataProvider(new MSSqlRepository());
    var record = (await dataProvider.ReadRecordAsync(database, recordId, cancellationToken))!;

    var notesField = "notes";
    var notes = new List<string>();
    var repCount = record.RepCount(notesField);
    for (int occ = 1; occ <= repCount; occ++)
    {
      if (record[notesField, occ] is { } note)
      {
        notes.Add(note);
      }
    }

    return notes;
  }

  public static List<string> GetMediaRecordIds(Record record)
  {
    var database = record.Database!;

    // condition for backwards compatibility: in model application 5.2 field is called media.reference.lref
    var imageField = database.FindFieldByTagOrName("media.reference.lref")
      ?? database.FindFieldByTagOrName("reproduction.reference.lref");

    var result = new List<string>();
    if (imageField is not null)
    {
      var occurrences = record.RepCount(imageField);
      for (int occ = 1; occ <= occurrences; occ++)
      {
        var value = record[imageField, occ];
        if (value is not null)
        {
          result.Add(value);
        }
      }
    }

    return result;
  }

  public static List<string>? GetHazardFields(Record record,
                                              string? language = null)
  {
    string[] hazardFields =
    [
      "hazard",
      "hazard.date",
      "hazard.notes",
    ];

    var result = new List<string>();
    foreach (var field in hazardFields)
    {
      if (record.Database!.FindFieldByTagOrName(field) == null)
      {
        continue;
      }

      var repCount = record.RepCount(field);
      for (int occ = 1; occ <= repCount; occ++)
      {
        if (record[field, occ, language ?? ""] is { } hazard)
        {
          result.Add(hazard);
        }
      }
    }

    return result.Count > 0 ? result : null;
  }

  public static List<string>? GetRecommendationFields(Record record,
                                                      string? language = null)
  {
    string[] recommendationFields =
    [
      "recommendation.display",
      "recommendation.environment",
      "recommendation.handling",
      "recommendation.installation",
      "recommendation.packing",
      "recommendation.security",
      "recommendation.storage",
    ];

    var result = new List<string>();
    foreach (var field in recommendationFields)
    {
      if (record.Database!.FindFieldByTagOrName(field) == null)
      {
        continue;
      }

      var repCount = record.RepCount(field);
      for (int occ = 1; occ <= repCount; occ++)
      {
        if (record[field, occ, language ?? ""] is { } recommendation)
        {
          result.Add(recommendation);
        }
      }
    }

    return result.Count > 0 ? result : null;
  }

  public static async Task AddNoteToRecord(string folder,
                                           string databaseName,
                                           int recordId,
                                           string text,
                                           CancellationToken cancellationToken)
  {
    var database = OpenDatabase(folder, databaseName);
    var dataProvider = new DDataProvider(new MSSqlRepository());
    var notesField = "notes";

    var record = (await dataProvider.ReadRecordAsync(database, recordId, cancellationToken))!;
    record.Set(notesField, record.RepCount(notesField) + 1, text);
    await record.WriteAsync(cancellationToken);
  }

  public static async Task AddImageToObject(string folder,
                                            string objectDatabaseName,
                                            int objectId,
                                            Stream imageStream,
                                            CancellationToken cancellationToken)
  {
    var database = OpenDatabase(folder, objectDatabaseName);
    var dataProvider = new DDataProvider(new MSSqlRepository());
    var record = (await dataProvider.ReadRecordAsync(database, objectId, cancellationToken))!;

    // condition for backwards compatibility: in model application 5.2 field is called media.reference
    var imageField = database.FindFieldByTagOrName("media.reference") is not null
      ? "media.reference"
      : "reproduction.reference";

    var tempPath = Path.GetTempPath();
    var fileName = Guid.NewGuid() + ".jpg";
    var filePath = Path.Join(tempPath, fileName);
    using var fileStream = File.Create(filePath);

    await imageStream.CopyToAsync(fileStream, cancellationToken);
    fileStream.Close();

    record.Set(imageField, record.RepCount(imageField) + 1, filePath);
    await record.WriteAsync(cancellationToken);
  }

  public static async Task<Result> AddConditionToObject(string folder,
                                                        string databaseName,
                                                        int recordId,
                                                        CancellationToken cancellationToken,
                                                        string? part = null,
                                                        string? condition = null,
                                                        string? notes = null,
                                                        string? checkName = null,
                                                        DateOnly? date = null)
  {
    var database = OpenDatabase(folder, databaseName);
    if (!database.SupportsConditions)
    {
      return Result.CreateErrorResult(MessageSeverityEnum.Error, ErrorCodeEnum.ConditionIsNotSupported, databaseName);
    }

    var conditionFields = database.ConditionFields!;

    var dataProvider = new DDataProvider(new MSSqlRepository());
    var record = (await dataProvider.ReadRecordAsync(database, recordId, cancellationToken))!;

    var newCondition = new ConditionData
    {
      Part = part,
      Condition = condition,
      Notes = notes,
      CheckName = checkName,
      Date = date
    };

    ConditionData.SetRow(record, conditionFields, newCondition);
    await record.WriteAsync(new SqlStateInfo());
    return Result.CreateSuccessResult();
  }

  /// <summary>
  /// for debugging purposes
  /// </summary>
  /// <param name="record"></param>
  /// <param name="fields"></param>
  /// <returns></returns>
  internal static List<LocationData> ShowRows(Record record, LocationFieldData fields)
  {
    var lastOcc = record.RepCount(fields.Id!);
    List<LocationData> rows = [];
    for (int occ = 1; occ <= lastOcc; occ++)
    {
      var row = LocationData.GetRow(record, fields, occ);
      rows.Add(row);
    }
    return rows;
  }
}
