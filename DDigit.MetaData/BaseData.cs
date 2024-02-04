namespace DDigit.MetaData;

public abstract class BaseData
{
    protected BaseData(ObjectTypeEnum objectType, string? fileName)
    {
        ObjectType = objectType;
        FileName = fileName;
    }

    protected BaseData(ObjectTypeEnum objectType, Stream? stream, Encoding encoding, string? fileName, PropertyList properties, bool trace) : this(objectType, fileName)
    {
        if (stream != null)
        {
            ReadProperties(this, properties, stream, encoding, fileName, trace);
        }
    }

    protected BaseData(ObjectTypeEnum objectType, PropertyList properties)
    {
        ObjectType = objectType;
        ElementCount = properties.Max(property => property.Element);
        FileName = null;
    }

    protected void SetProperty(PropertyMap property, object metadataObject, object? value, bool trace)
    {
        if (trace)
        {
            Debug.WriteLine($"{property.Position:n0} Element: {property.Element} {property.Name} {property.DataType}  = '{value}'");
        }

        if (property.Name != null)
        {
            var propertyInfo = metadataObject.GetType().
              GetProperty(property.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ??
              throw new DDException($"Object {ObjectType} does not have a property with name {property.Name}.");

            propertyInfo.SetValue(metadataObject, value);
        }
    }

    protected object? GetProperty(object metadataObject, string? name)
    {
        object? result = null;
        if (name != null)
        {
            var property = metadataObject.GetType().
              GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ??
              throw new DDException($"Object {ObjectType} does not have a property with name {name}.");
            result = property.GetValue(metadataObject);
        }
        return result;
    }

    internal void ReadProperties(BaseData baseData, PropertyList structure, Stream stream, Encoding encoding, string? fileName, bool trace)
    {
        foreach (var property in structure)
        {
            property.Position = 0;
        }

        var lastObject = structure.Max(p => p.Element);
        if (trace)
        {
            Debug.WriteLine($"---- Reading {baseData.ObjectType} ({lastObject})----");
        }

        foreach (var property in structure)
        {
            if (baseData.ElementCount.HasValue)
            {
                if (property.Element > baseData.ElementCount.Value)
                {
                    break;
                }
            }

            property.Position = stream.Position;

            SetProperty(property, baseData, stream.ReadObject(property, encoding), trace);
            if (property.Element == lastObject && baseData.ElementCount > lastObject)
            {
                throw new InvalidDataException($"Structure {baseData.GetType().Name} ({baseData}) in '{fileName}' contains {lastObject} elements, but data has {baseData.ElementCount}, " +
                                               $"last object was '{property.Name}' with value '{GetProperty(baseData, property.Name)}', position = {stream.Position:n0}");
            }
        }
    }

    internal void WriteProperties(BaseData baseData, PropertyList properties, (PropertyList, IEnumerable<object>)[]? objectLists,
                                   Stream stream, Encoding encoding)
    {
        stream.WriteEnum(baseData.ObjectType);
        foreach (var property in properties)
        {
            if (property.Element > baseData.ElementCount)
            {
                break;
            }
            if (property.Name != null)
            {
                var item = GetProperty(baseData, property.Name);
                if (item != null)
                {
                    try
                    {
                        stream.WriteObject(property, item, encoding);
                    }
                    catch (InvalidCastException ex)
                    {
                        throw new InvalidDataException($"Property '{property.Name}' in object '{baseData.ObjectType}' cannot be converted to the right type in file {baseData.FileName}\n{ex.Message}'");
                    }
                }
                else
                {
                    throw new InvalidDataException($"Property '{property.Name}' not found in object '{baseData.ObjectType}' in file {baseData.FileName}");
                }
            }
        }
        WriteChildren(objectLists, stream, encoding);
    }

    internal void WriteChildren((PropertyList, IEnumerable<object>)[]? children, Stream stream, Encoding encoding)
    {
        if (children != null)
        {
            foreach (var (structure, list) in children)
            {
                foreach (var baseObject in list)
                {
                    if (baseObject is BaseData baseData)
                    {
                        WriteProperties(baseData, structure, baseData.Children, stream, encoding);
                    }
                }
            }
        }
    }

    /// <summary>
    /// The file name of the database information
    /// </summary>
    [JsonIgnore]
    public string? FileName
    {
        get; set;
    }

    /// <summary>
    /// The number of elements in this object
    /// </summary>
    internal short? ElementCount
    {
        get; set;
    }

    /// <summary>
    /// The object type for this object.
    /// </summary>
    internal ObjectTypeEnum ObjectType
    {
        get; set;
    }

    internal virtual (PropertyList, IEnumerable<object>)[]? Children
    {
        get;
    }
}
