namespace DDigit.Data;

public static class RecordMetaData
{
  struct EditData
  {
    internal string Date;
    internal string Time;
    internal string Source;
    internal string User;
    internal string? Notes;
  }

  public static void SetInputEditMetaData(Record record, string? notes = null)
  {
    static void SetEditGroup(Record record, EditFieldData group, int occ, EditData editData)
    {
      if (group.Date != null)
      {
        record[group.Date, occ] = editData.Date;
      }
      if (group.Time != null)
      {
        record[group.Time, occ] = editData.Time;
      }
      if (group.Source != null)
      {
        record[group.Source, occ] = editData.Source;
      }
      if (group.Name != null)
      {
        record[group.Name, occ] = editData.User;
      }
      if (group.Notes != null)
      {
        record[group.Notes, occ] = editData.Notes;
      }
    }

    static void SetEditMetaData(Record record, EditFieldData edit, EditData editData)
    {
      if (edit.Date is not null)
      {
        for (int occ = record.RepCount(edit.Date) + 1; occ > 1; occ--)
        {
          SetEditGroup(record, edit, occ, new EditData
          {
            Date = record[edit.Date, occ - 1]!,
            Time = record[edit.Time!, occ - 1]!,
            Source = record[edit.Source!, occ - 1]!,
            User = record[edit.Name!, occ - 1]!,
            Notes = record[edit.Notes!, occ - 1]
          });
        }

        SetEditGroup(record, edit, 1, editData);
      }
    }

    /// <summary>
    /// Sets the edit history metadata for an existing record with edit history.
    /// This goes into 3 groups of fields, input, edit, and edit history.
    /// </summary>
    /// <param name="record"></param>
    static void SetEditHistoryMetaData(Record record, EditFieldData edit, EditFieldData history, EditData editData)
    {
      if (edit.Date != null && history.Date != null)
      {
        if (record[edit.Date] == null)
        {
          SetEditGroup(record, edit, 1, editData);
        }
        else
        {
          if (record[edit.Date] == editData.Date)
          {
            record[edit.Time!] = editData.Time;
          }
          else
          { // update the last occurrence
            var occ = record.RepCount(history.Date);
            if (occ > 0 && record[history.Date, occ] == record[edit.Date])
            {
              record[edit.Time!] = editData.Time;
            }
            else
            { // add a new occurrence, copy existing editData to the group}
              occ++;
              SetEditGroup(record, history, occ, new EditData
              {
                Date = record[edit.Date]!,
                Time = record[edit.Time!]!,
                Source = record[edit.Source!]!,
                User = record[edit.Name!]!,
                Notes = record[edit.Notes!]
              });
              SetEditGroup(record, edit, 1, editData);
            }
          }
        }
      }
    }

    var database = record.Database!;
    var inputGroup = database.InputGroup;
    var editGroup = database.EditGroup;

    if (inputGroup.Date != null && editGroup.Date != null)
    {
      var now = DateTime.Now;
      var editHistoryGroup = database.EditHistoryGroup;

      var editData = new EditData
      {
        Date = Date(editGroup.Date!.Type, now),
        Time = $"{now:HH:mm:ss}",
        Source = database + (record.Dataset?.Name != null ? $">{record.Dataset?.Name}" : string.Empty),
        User = record.User ?? Environment.UserName,
        Notes = notes
      };

      if (record.Id == 0 && record[inputGroup.Date!] is null)
      {
        SetEditGroup(record, inputGroup, 1, editData);  // for a new record
      }
      else
      {
        if (record.Database!.EditHistoryGroup.Date is null)
        {
          SetEditMetaData(record, editGroup, editData); // for an existing record without an edit history
        }
        else
        {
          SetEditHistoryMetaData(record, editGroup, editHistoryGroup, editData); // for an existing record with edit history
        }
      }
    }
  }

  private static string Date(FieldTypeEnum type, DateTime now)
    => type switch
    {
      FieldTypeEnum.DateIso => $"{now:yyyy-MM-dd}",
      _ => throw new InvalidDataException($"Invalid date type {type}"),
    };

}
