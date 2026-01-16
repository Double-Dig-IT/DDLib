namespace DDigit.MetaData;

/// <summary>
/// Different types of objects on screens / tabs
/// </summary>
public enum FormObjectTypeEnum : short
{
  /// <summary>
  /// Unknown object (should not happen)
  /// </summary>
  Undefined,

  /// <summary>
  /// A label
  /// </summary>
  Label,

  /// <summary>
  /// Used to control external devices
  /// </summary>
  [Obsolete("was used in Prime Adlib")]
  AuxiliaryData,

  /// <summary>
  /// Data
  /// </summary>
  Data,

  /// <summary>
  /// Menu option
  /// </summary>
  [Obsolete("No longer in use in Windows Adlib and higher")]
  MenuOption,

  /// <summary>
  /// A variable length piece of text
  /// </summary>
  [Obsolete("No longer in use in Windows Adlib and higher")]
  TextWindow,

  /// <summary>
  /// An input field that is not bound to a database field
  /// </summary>
  [Obsolete("No longer in use in Windows Adlib and higher")]
  Input,

  /// <summary>
  /// Some sort of system variable
  /// </summary>
  [Obsolete("No longer in use in Windows Adlib and higher")]
  System,

  /// <summary>
  /// A rectangular area on the screen.
  /// </summary>
  Box,

  /// <summary>
  /// A rectangular area on the screen, that needs to be cleared.
  /// </summary>
  [Obsolete("No longer in use in Windows Adlib and higher")]
  ClearArea,

  /// <summary>
  /// A parameter to control the behavior of the screen.
  /// </summary>
  Parameter,

  /// <summary>
  /// Output to a serial port...
  /// </summary>
  [Obsolete("was used in Prime Adlib")]
  AuxPortOutput,

  /// <summary>
  /// Rectangular area that holds an image.
  /// </summary>
  Image,

  /// <summary>
  /// A rectangular data that holds a web browser window.
  /// </summary>
  WebBrowser,

  /// <summary>
  /// A button
  /// </summary>
  [Obsolete("No longer in use in Windows Adlib and higher")]
  Button,

  /// <summary>
  /// A field that contains HTML
  /// </summary>
  HtmlField
}
