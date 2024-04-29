using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace UndefinedCore;

public static partial class VisualStudioUtility
{
   public static void ValidateVSWhereIsAvailabe()
   {
      if (!File.Exists(GetVSWherePath()))
      {
         throw new InvalidOperationException($"Expected vswhere.exe to be at \"{GetVSWherePath()}\"");
      }

      ProcessStartInfo processStartInfo = GetVSWhereProcessStartInfo();
      Process? proc;
      try
      {
         proc = Process.Start(processStartInfo);
      }
      catch (Exception)
      {
         throw new InvalidOperationException("Failed to start vswhere.exe");
      }

      proc!.WaitForExit();
      if (proc.ExitCode != 0)
      {
         throw new InvalidOperationException("Failed to start vswhere.exe");
      }
   }

   public static string GetCompilerBinariesDirectory(VisualStudioProductInfo visualStudioProductInfo, Version compilerVersion, Architecture architecture)
   {
      string installationPath = visualStudioProductInfo.InstallationPath;
      string hostFolder = Environment.Is64BitOperatingSystem ? "Hostx64" : "Hostx86";

      string architectureFolder = architecture switch
      {
         Architecture.x86 => "x86",
         Architecture.x64 => "x64",
         Architecture.ARM => "arm",
         Architecture.ARM64 => "arm64",
         _ => throw new ArgumentOutOfRangeException(nameof(architecture))
      };

      return Path.Combine(installationPath, "VC", "Tools", "MSVC", compilerVersion.ToString(), "bin", hostFolder, architectureFolder);
   }

   public static IEnumerable<Version> GetInstalledCompilerVersions(VisualStudioProductInfo visualStudioProductInfo)
   {
      string installedMsvcPath = Path.Combine(visualStudioProductInfo.InstallationPath, "VC", "Tools", "MSVC");
      string[] dirs = Directory.GetDirectories(installedMsvcPath);
      for (int i = 0; i < dirs.Length; i++)
      {
         string dir = dirs[i].TrimEnd(Path.DirectorySeparatorChar);
         string lastPart = Path.GetFileName(dir);
         if (Version.TryParseSemanticVersionSimple(lastPart, out Version semanticVersion))
         {
            yield return semanticVersion;
         }
      }
   }

   public static VisualStudioProductInfo[] GetInstalledProductInfos()
   {
      string output = ExecuteVSWhere("-products * -format json");
      VisualStudioProductInfo[]? visualStudioProductInfos = JsonConvert.DeserializeObject<VisualStudioProductInfo[]>(output);
      return visualStudioProductInfos ?? [];
   }

   private static string ExecuteVSWhere(string arguments)
   {
      ProcessStartInfo processStartInfo = GetVSWhereProcessStartInfo();
      processStartInfo.Arguments = arguments;
      using Process? proc = Process.Start(processStartInfo);
      if (proc == null)
      {
         throw new InvalidOperationException("Failed to start vswhere.exe");
      }

      proc.WaitForExit();
      if (proc.ExitCode != 0)
      {
         throw new InvalidOperationException("Failed to start vswhere.exe");
      }

      return proc.StandardOutput.ReadToEnd();
   }

   private static ProcessStartInfo GetVSWhereProcessStartInfo()
   {
      return new()
      {
         FileName = GetVSWherePath(),
         UseShellExecute = false,
         RedirectStandardOutput = true,
         CreateNoWindow = true,
         RedirectStandardError = true
      };
   }

   private static string GetVSWherePath()
   {
      string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
      return Path.Combine(programFilesPath, "Microsoft Visual Studio", "Installer", "vswhere.exe");
   }
}
