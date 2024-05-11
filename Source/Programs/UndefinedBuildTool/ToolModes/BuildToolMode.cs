using BandoWare.Core;
using System;
using System.IO;

namespace BandoWare.UndefinedBuildTool;

[ToolMode("Build")]
public class BuildToolMode : ToolMode
{
   [CommandLine("-BuildProject", ValueUsage = "<ProjectFilePath>", Description = "The project to build.")]
   public string? ProjectFilePath { get; set; }

   [CommandLine("-Architecture", ValueUsage = "$EnumValues", Description = "The architecture to build for.")]
   public Architecture? Architecture { get; set; }

   [CommandLine("-Platform", ValueUsage = "$EnumValues", Description = "The platform to build for.")]
   public Platform? Platform { get; set; }

   private ScopedLogger m_Logger;

   public BuildToolMode(CommandLineArguments commandLineArguments, ILogger logger)
   {
      commandLineArguments.ApplyTo(this);
      m_Logger = logger.CreateScope(nameof(BuildToolMode));
   }

   public override void Execute()
   {
      if (!File.Exists(ProjectFilePath))
         throw new InvalidOperationException("Project file does not exist.");

      //if (!ProjectFilePath.EndsWith(".Build.cs"))
      //{
      //   throw new InvalidOperationException("Project file is not a build script.");
      //}

      try
      {
         Architecture ??= ArchitectureUtility.GetHostArchitecture();
         Platform ??= PlatformUtility.GetHostPlatform();
      }
      catch (Exception exception)
      {
         Console.WriteLine("Failed to determine host architecture and platform.");
         m_Logger.LogException(LogLevel.Error, exception);
      }

      foreach (VisualStudioProductInfo vsProductInfo in VisualStudioUtility.GetInstalledProductInfos())
      {
         VisualStudioUtility.GetInstalledCompilerVersions(vsProductInfo);

         foreach (Core.Version compilerVersion in VisualStudioUtility.GetInstalledCompilerVersions(vsProductInfo))
         {
            //string compilerPath = VisualStudioUtility.GetCompilerBinariesDirectory(vsProductInfo, compilerVersion);
            //Console.WriteLine(compilerPath);
         }
      }

      throw new NotImplementedException();
   }
}
