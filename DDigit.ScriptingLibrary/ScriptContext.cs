using DDigit.Exceptions;
using Python.Runtime;

namespace DDigit.ScriptingLibrary;
public static class ScriptContext
{
  public static bool IsInitialized { get; private set; } = false;

  private static readonly Lock pythonLock = new();

  public static void Initialize()
  {
    if (!IsInitialized)
    {
      lock (pythonLock)
      {
        var baseDir = AppContext.BaseDirectory;
        var pythonPath = Path.Combine(baseDir, "python");
        var dllName = GetPythonDllName(pythonPath);

        Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", dllName);

        PythonEngine.PythonHome = pythonPath;
        PythonEngine.Initialize();
        PythonEngine.BeginAllowThreads();
        PythonEngine.DebugGIL = false; // Disable GIL debugging for performance

        IsInitialized = true;
      }
    }
  }

  private static string GetPythonDllName(string pythonPath)
  {
    const int majorVersion = 3;

    var directoryInfo = new DirectoryInfo(pythonPath);
    if (!directoryInfo.Exists)
    {
      throw new DDException($"Python folder '{pythonPath}' not found.");
    }

    var dll = directoryInfo.GetFiles($"python{majorVersion}*.dll").Where(f => f.Name != $"python{majorVersion}.dll").ToArray();
    if (dll.Length == 0)
    {
      throw new DDException($"Python DLL not found in '{pythonPath}'.");
    }

    if (dll.Length > 1)
    {
      throw new DDException($"More than one Python DLL found in '{pythonPath}'.");
    }

    return dll[0].FullName;
  }
}
