using DDigit.Classes;

namespace DDigit.ScriptingLibrary;

using DDigit.Exceptions;
using Python.Runtime;
using System;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Runtime.InteropServices;

public static class ScriptHelper
{
  public static void RunPowerShell(string scriptPath, object record)
  {
    if (string.IsNullOrWhiteSpace(scriptPath))
    {
      throw new ArgumentNullException(nameof(scriptPath));
    }

    if (!File.Exists(scriptPath))
    {
      throw new DDException($"PowerShell script '{scriptPath}' not found.");
    }

    string cachedScriptPath = GetCachedScriptPath(scriptPath);

    // Copy script to per-user roaming folder if not already cached
    if (!File.Exists(cachedScriptPath) ||
        File.GetLastWriteTimeUtc(cachedScriptPath) < File.GetLastWriteTimeUtc(scriptPath))
    {
      Directory.CreateDirectory(Path.GetDirectoryName(cachedScriptPath)!);
      File.Copy(scriptPath, cachedScriptPath, overwrite: true);

      // On Windows: remove Zone.Identifier
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        try
        {
          File.Delete(cachedScriptPath + ":Zone.Identifier");
        }
        catch
        {
          // Silently ignore
        }
      }
    }

    using var runSpace = RunspaceFactory.CreateRunspace();
    runSpace.Open();

    using var pipeline = runSpace.CreatePipeline();
    var command = new Command(cachedScriptPath);
    command.Parameters.Add("Record", record);
    pipeline.Commands.Add(command);

    pipeline.Invoke();
  }

  private const string redirectorCode =
    """
    import sys
    import clr

    class OutputCatcher:
        def write(self, text):
            if text.strip():
                dotnet_callback(text)
        def flush(self):
            pass

    catcher = OutputCatcher()
    sys.stdout = catcher
    sys.stderr = catcher
    """;


  public static class PythonOutputInterceptor
  {
    public static void RunPythonWithOutputCapture(string script, Action<string> onOutput)
    {
      using (Py.GIL())
      {
        dynamic sys = Py.Import("sys");

        // Create a Python class for redirecting stdout

        PythonEngine.Exec(redirectorCode);

        dynamic outputCatcher = PythonEngine.Eval("OutputCatcher()");
        sys.stdout = outputCatcher;
        sys.stderr = outputCatcher;

        // Register the callback in C#
        var catcherType = outputCatcher.GetPythonType();
        catcherType.SetAttr("WriteToHost", new Action<string>(onOutput).ToPython());

        // Now run your actual script
        PythonEngine.Exec(script);

        // Reset stdout/stderr if you like
        sys.stdout = sys.__stdout__;
        sys.stderr = sys.__stderr__;
      }
    }
  }

  private static readonly Lock pythonLock = new();

  public static void RunPython(string scriptPath, object record)
  {
    if (!File.Exists(scriptPath))
    {
      throw new DDException($"Python script '{scriptPath}' not found.");
    }

    ScriptContext.Initialize();

    lock (pythonLock)
    {
      using (Py.GIL())
      {
        try
        {
          dynamic pyMain = Py.Import("__main__");
          pyMain.record = record.ToPython();
          string script = File.ReadAllText(scriptPath);
          PythonEngine.Exec(script);
        }
        catch (PythonException ex)
        {
          throw new DDException($"Error executing Python script '{scriptPath}': {ex.Message}", ex);
        }
      }
    }
  }

  public static void RunPython(string scriptPath, object record, ScriptTriggerCodeEnum triggerCode, Action<string> onOutput)
  {
    if (!File.Exists(scriptPath))
    {
      throw new DDException($"Python script '{scriptPath}' not found.");
    }

    ScriptContext.Initialize();

    using (Py.GIL())
    {
      using var scope = Py.CreateScope();
      // Make .NET callback available in Python
      var action = new Action<string>(onOutput).ToPython();
      scope.Set("dotnet_callback", action);

      // Define OutputCatcher and assign to sys.stdout/stderr
      scope.Exec(redirectorCode);

      // Inject your .NET record object
      scope.Set("record", record.ToPython());
      scope.Set("trigger_code", (int)triggerCode);

      // Load and execute actual user Python script
      string script = File.ReadAllText(scriptPath);
      scope.Exec(script);

      // Optionally restore stdout/stderr
      dynamic sys = Py.Import("sys");
      sys.stdout = sys.__stdout__;
      sys.stderr = sys.__stderr__;
      action.Dispose();
    }
  }


  private static string GetCachedScriptPath(string originalPath)
  {
    // Sanitize filename for use in user profile
    string fileName = Path.GetFileName(originalPath);
    string hash = originalPath.GetHashCode().ToString("X8"); // basic hash to prevent collisions
    string cacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DDigit",
        "ScriptCache");

    return Path.Combine(cacheDir, $"{hash}_{fileName}");
  }
}

