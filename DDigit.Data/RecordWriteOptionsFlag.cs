namespace DDigit.Data;

/// <summary>
/// Options to determine the record write behaviour
/// </summary>
[Flags]
public enum RecordWriteOptionsFlag
{
    /// <summary>
    /// No options set, do everything
    /// </summary>
    None = 0,

    /// <summary>
    /// Do not update the record history
    /// </summary>
    NoEditHistory = 1,

    /// <summary>
    /// No not calculate any autonumbering fields
    /// </summary>
    NoAutoNumbering = 2,

    /// <summary>
    /// Do not execute any scripts, (no Adapl, Powershell or Python)
    /// </summary>
    NoScripts = 4,

    /// <summary>
    /// Do not upload any (image files)
    /// </summary>
    NoFilesUpload = 8,
}