using BandoWare.Core;
using System.Linq;

namespace BandoWare.UndefinedBuildTool;

public class ToolModeOptions
{
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
      ValueUsage = $"$Function:{nameof(GetModeValueUsage)}",
      Description = "The mode to run."
   )]
   public string ToolMode { get; set; } = "Help";

   private static string GetModeValueUsage()
   {
      string[] toolModeNames = ToolModeUtility.GetAllToolModeNames();
      return string.Join(" | ", toolModeNames.Select(v => $"\"{v}\""));
   }
}
