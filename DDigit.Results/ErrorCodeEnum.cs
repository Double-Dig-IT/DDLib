namespace DDigit.Results;

/// <summary>
/// An enumeration that contains error codes.
/// </summary>
public enum ErrorCodeEnum
{
  /// <summary>
  /// No error.
  /// </summary>
  NoError,

  /// <summary>
  /// Trying to write a repeated value in a non repeated field.
  /// </summary>
  FieldIsNotRepeated,

  /// <summary>
  /// Data is too long for the field.
  /// </summary>
  FieldValueIsTooLong,

  /// <summary>
  /// Creation of new terms is not allowed
  /// </summary>
  ForcingIsNotAllowed,

  /// <summary>
  /// Data is not a valid ISO data
  /// </summary>
  InvalidIsoDate,

  /// <summary>
  /// Data is not a valid integer
  /// </summary>
  InvalidInteger,

  /// <summary>
  /// Data is not a valid floating point number
  /// </summary>
  InvalidNumeric,

  /// <summary>
  /// Move transactions are not supported for this database.
  /// </summary>
  MoveIsNotSupported,

  /// <summary>
  /// Package is already at this location.
  /// </summary>
  PackageIsAlreadyAtThisLocation,

  /// <summary>
  /// Location record could not be found.
  /// </summary>
  LocationRecordNotFound,

  /// <summary>
  /// Object is already at this location.
  /// </summary>
  ObjectIsAlreadyAtThisLocation,

  /// <summary>
  /// ObjectRecordNotFound
  /// </summary>
  ObjectRecordNotFound,

  /// <summary>
  /// Cannot move location, only packages/containers can be moved
  /// </summary>
  CannotMoveLocation,

  /// <summary>
  /// The record does not support move history
  /// </summary>
  MoveHistoryNotSupported,

  /// <summary>
  /// Conditions are not supported for this database
  /// </summary>
  ConditionIsNotSupported,

  /// <summary>
  /// If a record does not have a current location
  /// </summary>
  CurrentLocationNotFound,

  /// <summary>
  /// If an object does not have a home location
  /// </summary>
  HomeLocationNotFound,

  /// <summary>
  /// Home locations are not supported for this database
  /// </summary>
  HomeLocationNotSupported,

  /// <summary>
  /// Field is not found for this database
  /// </summary>
  FieldNotFound,
  /// <summary>
  /// No linked locations are found for this record (inside of part_of.lref)
  /// </summary>
  LinkedLocationsNotFound
}

