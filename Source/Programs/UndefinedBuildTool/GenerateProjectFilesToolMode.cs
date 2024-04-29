using System;
using UndefinedCore;

namespace UndefinedBuildTool;

[ToolMode("GenerateProjectFiles")]
public class GenerateProjectFilesToolMode : ToolMode
{
   [CommandLine("-VS2022", Value = "VisualStudio2022", Description = "Generates project files for Visual Studio 2022.")]
   [CommandLine("-VS2019", Value = "VisualStudio2019", Description = "Generates project files for Visual Studio 2019.")]
   [CommandLine("-VS2017", Value = "VisualStudio2017", Description = "Generates project files for Visual Studio 2017.")]
   [CommandLine("-VSLatest", Value = "VisualStudioLatest", Description = "Generates project files for the latest version of Visual Studio.")]
   public Editor TargetEditor { get; set; } = Editor.None;

   private ScopedLogger m_Logger;

   public GenerateProjectFilesToolMode(ILogger logger, CommandLineArguments commandLineArguments)
   {
      commandLineArguments.ApplyTo(this);
      m_Logger = logger.CreateScope("GenerateProjectFiles");
   }

   public override void Execute()
   {
      if (TargetEditor == Editor.None)
      {
         Console.WriteLine("No target specified. Use -Help for usage information.");
         m_Logger.Log(LogLevel.Error, "No target specified.");
         return;
      }

      throw new NotImplementedException();
   }
}
