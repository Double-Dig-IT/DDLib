namespace DDigit.Exceptions;

public class MissingRelationTagException(string relation, string tag) : DDException($"Missing tag for relation type {relation} for tag '{tag}'")
{

  public string Relation { get; private set; } = relation;

  public string Tag { get; private set; } = tag;
}

