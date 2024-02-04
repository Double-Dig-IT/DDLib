namespace DDigit.MetaData;

internal static class Extensions
{
  static Extensions()
  {
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    WindowsEncoding = Encoding.GetEncoding("windows-1252");
    DosEncoding = Encoding.GetEncoding("ibm850");
  }

  public static readonly Encoding WindowsEncoding;

  public static readonly Encoding DosEncoding;


  internal static short ReadInt16(this Stream stream)
  {
    var buffer = new Span<byte>(new byte[2]);
    stream.Read(buffer);
    return (short)(buffer[0] | buffer[1] << 8);
  }

  internal static void WriteInt16(this Stream stream, int value)
  {
    stream.WriteByte((byte)(value & 0xff));
    stream.WriteByte((byte)((value >> 8) & 0xff));
  }

  internal static int ReadInt32(this Stream stream)
  {
    var buffer = new Span<byte>(new byte[4]);
    stream.Read(buffer);
    return buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24;
  }

  internal static uint ReadUInt32(this Stream stream) => (uint)stream.ReadInt32();

  internal static float ReadFloat(this Stream stream)
  {
    var buffer = new Span<byte>(new byte[4]);
    stream.Read(buffer);
    return BitConverter.ToSingle(buffer);
  }

  internal static void WriteFloat(this Stream stream, float value)
  {
    var buffer = BitConverter.GetBytes(value);
    stream.Write(buffer, 0, buffer.Length);
  }

  internal static void WriteUInt32(this Stream stream, uint value) => stream.WriteInt32((int)value);

  internal static void WriteInt32(this Stream stream, int value)
  {
    stream.WriteByte((byte)(value & 0xff));
    stream.WriteByte((byte)((value >> 8) & 0xff));
    stream.WriteByte((byte)((value >> 16) & 0xff));
    stream.WriteByte((byte)((value >> 24) & 0xff));
  }

  internal static string ReadString(this Stream stream, Encoding encoding)
  {
    var bytes = new List<byte>(256);
    var ch = (byte)stream.ReadByte();
    while (ch != 0)
    {
      bytes.Add(ch);
      ch = (byte)stream.ReadByte();
    }
    return encoding.GetString(bytes.ToArray());
  }

  internal static void WriteString(this Stream stream, Encoding encoding, string value)
  {
    foreach (var ch in encoding.GetBytes(value))
    {
      stream.WriteByte(ch);
    }
    stream.WriteByte(0);
  }

  internal static string ReadFixedLengthString(this Stream stream, int bytes)
  {
    var result = new StringBuilder();
    for (int i = 0; i < bytes; i++)
    {
      var ch = (char)stream.ReadByte();
      if (ch == 0)
      {
        break;
      }
      result.Append(ch);
    }
    return result.ToString();
  }

  internal static void WriteFixedLengthString(this Stream stream, string text, int bytes)
  {
    for (int i = 0; i < bytes; i++)
    {
      stream.WriteByte(i < text.Length ? (byte)text[i] : (byte)0);
    }
  }

  internal static bool ReadBool(this Stream stream) => stream.ReadInt16() != 0;

  internal static bool ReadBool32(this Stream stream) => stream.ReadInt32() != 0;

  internal static void WriteBool(this Stream stream, bool value) => stream.WriteInt16(value ? 1 : 0);

  internal static void WriteBool32(this Stream stream, bool value) => stream.WriteInt32(value ? 1 : 0);

  internal static object ReadEnum(this Stream stream, Type? enumType)
    => Enum.ToObject(enumType!, stream.ReadInt16());

  internal static void WriteEnum(this Stream stream, object value) =>
     stream.WriteInt16(Convert.ToInt16(value));

  internal static object ReadEnum32(this Stream stream, Type? enumType)
   => Enum.ToObject(enumType!, stream.ReadInt32());

  internal static void WriteEnum32(this Stream stream, object value) =>
     stream.WriteInt32(Convert.ToInt32(value));

  private static readonly char[] csvSeparators = [',', ' '];
  internal static string[]? ReadCsv(this Stream stream, Encoding encoding)
    => ReadString(stream, encoding)?.Split(csvSeparators, StringSplitOptions.RemoveEmptyEntries);

  internal static void WriteCsv(this Stream stream, Encoding encoding, object value)
    => WriteString(stream, encoding, string.Join(",", (string[])value));

  internal static Guid? ReadGuid(this Stream stream, Encoding encoding)
  {
    string guidString = ReadString(stream, encoding);
    return string.IsNullOrWhiteSpace(guidString) ? null : Guid.Parse(guidString);
  }

  internal static object? ReadObject(this Stream stream, PropertyMap propertyMap, Encoding encoding)
    => propertyMap.DataType switch
    {
      DataTypesEnum.String => stream.ReadString(encoding),
      DataTypesEnum.Bool => stream.ReadBool(),
      DataTypesEnum.Int16 => stream.ReadInt16(),
      DataTypesEnum.Int32 => stream.ReadInt32(),
      DataTypesEnum.Enum => stream.ReadEnum(propertyMap.Type),
      DataTypesEnum.Enum32 => stream.ReadEnum32(propertyMap.Type),
      DataTypesEnum.BoolI => !stream.ReadBool(),  // invert the boolean from disk... 
      DataTypesEnum.Skip => null,             // do not read anything
      DataTypesEnum.Guid => stream.ReadGuid(encoding),
      DataTypesEnum.Bool32 => stream.ReadBool32(),
      DataTypesEnum.Bool32I => !stream.ReadBool32(),
      DataTypesEnum.UInt32 => stream.ReadUInt32(),
      DataTypesEnum.Float => stream.ReadFloat(),
      _ => null,
    };

  internal static void WriteObject(this Stream stream, PropertyMap propertyMap, object value, Encoding encoding)
  {
    switch (propertyMap.DataType)
    {
      case DataTypesEnum.Int16:
        stream.WriteInt16((short)value);
        break;

      case DataTypesEnum.Int32:
        stream.WriteInt32((int)value);
        break;

      case DataTypesEnum.String:
        stream.WriteString(encoding, (string)value);
        break;

      case DataTypesEnum.Bool:
        stream.WriteBool((bool)value);
        break;

      case DataTypesEnum.BoolI:
        stream.WriteBool(!(bool)value);
        break;

      case DataTypesEnum.Bool32:
        stream.WriteBool32((bool)value);
        break;

      case DataTypesEnum.Bool32I:
        stream.WriteBool32(!(bool)value);
        break;

      case DataTypesEnum.Enum:
        stream.WriteEnum(value);
        break;

      case DataTypesEnum.Enum32:
        stream.WriteEnum32(value);
        break;

      case DataTypesEnum.UInt32:
        stream.WriteUInt32((uint)value);
        break;

      case DataTypesEnum.Float:
        stream.WriteFloat((float)value);
        break;

      default:
        throw new InvalidOperationException($"Property {propertyMap} cannot be written");
    };
  }
}

