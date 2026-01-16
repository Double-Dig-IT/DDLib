using System.Runtime.CompilerServices;

namespace DDigit.MetaData;

/// <summary>
/// Base class for all objects, conntains code to read an write them.
/// </summary>
public abstract class BaseData
{
  /// <summary>
  /// Contructor to create a new object.
  /// </summary>
  /// <param name="objectType"></param>
  protected BaseData(ObjectTypeEnum objectType)
  {
    ObjectType = objectType;
  }

  /// <summary>
  /// Constructor to read the object from a a file.
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="stream"></param>
  /// <param name="encoding"></param>
  /// <param name="properties"></param>
  /// <param name="trace"></param>
  protected BaseData(ObjectTypeEnum objectType, FileStream? stream, Encoding encoding, IReadOnlyList<PropertyMap> properties, bool trace) :
    this(objectType)
  {
    if (stream is not null)
    {
      ReadProperties(this, properties, stream, encoding, trace);
    }
  }

  /// <summary>
  /// Contructor to create a new object, and setting the ElementCount
  /// based on the PRoperties list.
  /// </summary>
  /// <param name="objectType"></param>
  /// <param name="properties"></param>
  protected BaseData(ObjectTypeEnum objectType, PropertyList properties)
  {
    ObjectType = objectType;
    ElementCount = properties.Max(property => property.ElementIndex);
  }

  /// <summary>
  /// Set a property
  /// </summary>
  /// <param name="property"></param>
  /// <param name="metadataObject"></param>
  /// <param name="value"></param>
  /// <param name="trace"></param>
  /// <exception cref="DDException"></exception>
  protected void SetProperty(PropertyMap property, object metadataObject, object? value, bool trace)
  {
    if (trace)
    {
      Debug.WriteLine($"{property.Position:n0} Element: {property.ElementIndex} {property.Name} {property.DataType}  = '{value}'");
    }

    if (property.Name is not null)
    {
      var propertyInfo = metadataObject.GetType().
        GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ??
        throw new DDException($"Object {ObjectType} does not have a property with name {property.Name}.");

      propertyInfo.SetValue(metadataObject, value);
    }
    else
    {
      if (property.DataType is not DataTypesEnum.Skip)
      {
        throw new DDException("Property name is null");
      }
    }
  }

  /// <summary>
  /// Get a property
  /// </summary>
  /// <param name="metadataObject"></param>
  /// <param name="name"></param>
  /// <returns></returns>
  /// <exception cref="DDException"></exception>
  protected object? GetProperty(object metadataObject, string? name)
  {
    object? result;
    if (name is not null)
    {
      var property = metadataObject.GetType().
        GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ??
        throw new DDException($"Object {ObjectType} does not have a property with name {name}.");
      result = property.GetValue(metadataObject);
    }
    else
    {

      throw new DDException("Property name is null");
    }
    return result;
  }

  internal void ReadProperties(BaseData baseData, IReadOnlyList<PropertyMap> properties, FileStream stream, Encoding encoding, bool trace)
  {
    foreach (var property in properties)
    {
      property.Position = null;
    }

    var lastObject = properties.Max(p => p.ElementIndex);
    if (trace)
    {
      Debug.WriteLine($"---- Reading {baseData.ObjectType} ({lastObject})----");
    }

    foreach (var property in properties)
    {
      if (baseData.ElementCount is not null)
      {
        if (property.ElementIndex > baseData.ElementCount)
        {
          break;
        }
      }

      property.Position = stream.Position;

      SetProperty(property, baseData, stream.ReadObject(property, encoding), trace);
      if (property.ElementIndex == lastObject && baseData.ElementCount > lastObject)
      {
        throw new InvalidDataException($"Structure {baseData.GetType().Name} ({baseData}) in '{stream.Name}' contains {lastObject} elements, but data has {baseData.ElementCount}, " +
                                       $"last object was '{property.Name}' with value '{GetProperty(baseData, property.Name)}', position = {stream.Position:n0}");
      }
    }
  }

  internal void WriteProperties(BaseData baseData, IReadOnlyList<PropertyMap> properties, ChildrenList[] childrenList,
                                 FileStream stream, Encoding encoding)
  {
    void WriteChildren(ChildrenList[] children, FileStream stream, Encoding encoding)
    {
      if (children is not null)
      {
        foreach (var childList in children)
        {
          // skip element 0 from the list i when writing the texts for field names.
          for (int i = childList.Objects is FieldNameList or FormTitleList ? 1 : 0; i < childList.Objects.Count(); i++)
          {
            var childObject = childList.Objects.ElementAt(i);
            WriteProperties(childObject, childList.Properties, childObject.Children!, stream, encoding);
          }
        }
      }
    }

    stream.WriteEnum(baseData.ObjectType);
    foreach (var property in properties)
    {
      if (property.ElementIndex > baseData.ElementCount)
      {
        break;
      }
      if (property.Name is not null)
      {
        var item = GetProperty(baseData, property.Name);
        if (item is not null)
        {
          try
          {
            stream.WriteObject(property, item, encoding);
          }
          catch (InvalidCastException ex)
          {
            throw new InvalidDataException($"Property '{property.Name}' in object '{baseData.ObjectType}' cannot be converted to the right type in file {stream.Name}\n{ex.Message}'");
          }
        }
        else
        {
          throw new InvalidDataException($"Property '{property.Name}' not found in object '{baseData.ObjectType}' in file {stream.Name}");
        }
      }
    }
  
    WriteChildren(childrenList, stream, encoding);
  }

  /// <summary>
  /// Get the name of a property for a given type.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="_"></param>
  /// <param name="propertyName"></param>
  /// <returns></returns>
  protected static string PropertyName<T>(T _, [CallerMemberName] string propertyName = "")
    => propertyName;

  /// <summary>
  /// The number of elements in this object
  /// </summary>
  internal short? ElementCount
  {
    get; set;
  } = 1;  // Each object has at least one element, the element count itself

  /// <summary>
  /// The object type for this object.
  /// </summary>
  internal ObjectTypeEnum ObjectType
  {
    get; set;
  }

  internal virtual ChildrenList[]? Children
  {
    get;
  }

  /// <summary>
  /// Print the structure of this object
  /// For debugging purposes.
  /// </summary>
  /// <returns></returns>
  public string ObjectTree(int level = 0, int occ = 0)
  {
    var sb = new StringBuilder();
    for (int indent = 0; indent < level; indent++)
    {
      sb.Append("  ");
    }
    sb.AppendLine($"{(occ > 0 ? $"[{occ}]" : "")} Type: {ObjectType} {this}");
    if (Children is not null)
    {
      foreach (var childlist in Children)
      {
        if (childlist.Objects.Any())
        {
          for (int indent = 0; indent < level; indent++)
          {
            sb.Append("  ");
          }
          sb.AppendLine($"- {childlist} ({childlist.Objects.Count()})");
          int i = 1;
          foreach (var item in childlist.Objects)
          {
            sb.Append(item.ObjectTree(level + 1, i++));
          }
        }
      }
    }
    return sb.ToString();
  }
}
