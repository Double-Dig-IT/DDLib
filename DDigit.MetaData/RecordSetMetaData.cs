namespace DDigit.MetaData;

public class RecordSetMetaData
{
  public RecordSetMetaData()
  {

  }

  public RecordSetMetaData(string table, SqlDataReader reader)
  {
    Number = (int)reader["pfNumber"];
    Database = table;
    Title = (string)reader["title"];
    Owner = (string)reader["owner"];
    Selection = (string)reader["selectionStatement"];
    var hits = reader["hitCount"];
    Hits = hits != DBNull.Value ? (int) hits : 0;
    Created = GetDateTime(reader["creation"]);
    Modified = GetDateTime(reader["modification"]);
  }

  public RecordSetMetaData(int number, string database, string title, string owner, string selection, int hits, DateTime created, DateTime modified)
  {
    Number = number;
    Database = database;
    Title = title;
    Owner = owner;
    Selection = selection;
    Hits = hits;
    Created = created;
    Modified = modified;
  }

  public int Number
  {
    get; set;
  }

  public string Database
  {
    get; set;
  } = string.Empty;

  public string Title
  {
    get; set;
  } = string.Empty;

  public string Owner
  {
    get; set;
  } = string.Empty;

  public string Selection
  {
    get; set;
  } = string.Empty;

  public int Hits
  {
    get; set;
  } 

  public DateTime Created
  {
    get;
    set;
  } 

  public DateTime Modified
  {
    get;
    set;
  } 

  static DateTime GetDateTime(object dateTime) => dateTime != DBNull.Value ? (DateTime)dateTime : DateTime.MinValue;

  public override string ToString() => $"{Database} {Number} {Title} ({Hits})";

}
