using UndefinedCore;

namespace UndefinedBuildTool;

public class ToolModeOptions
{
   private readonly static string[] s_ModeOptions = ToolModeUtility.GetAllToolModeNames();

   [CommandLine
   (
      "-Build",
      Value = "Build",
      Description = "Builds the project. (Equivalent to -Mode \"Build\")"
   )]
   [CommandLine
   (
      "-GenerateProjectFiles",
      Value = "GenerateProjectFiles",
      Description = "Generates project files. (Equivalent to -Mode \"GenerateProjectFiles\")"
   )]
   [CommandLine
   (
      "-Help",
      Value = "Help",
      Description = "Displays this help. (Equivalent to -Mode \"Help\")"
   )]
   [CommandLine
   (
      "-Mode",
      ValueUsage = $"$StaticField:{nameof(s_ModeOptions)}",
      Description = "Generates project files."
   )]
   public string ToolMode { get; set; } = "Help";
}
