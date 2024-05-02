using BandoWare.Core;
using System;

namespace BandoWare.UndefinedBuildTool;

internal class UndefinedBuildTool
{
   public static void Main(string[] args)
   {
      Console.Title = nameof(UndefinedBuildTool);
      CommandLineArguments commandLineArguments;
      try
      {
         commandLineArguments = new(args);
      }
      catch (CommandLineParseException exception)
      {
         Console.WriteLine(exception.Message);
         return;
      }
      catch
      {
         Console.WriteLine("An unknown error occurred while parsing command line arguments. Use -Help for usage information.");
         return;
      }

      using Logger logger = new(commandLineArguments);
      Container container = new();

      container.Set<CommandLineArguments>(commandLineArguments);
      container.Set<ILogger>(logger);

      TryExecuteToolMode(commandLineArguments, logger, container);
   }

   private static void TryExecuteToolMode(CommandLineArguments commandLineArguments, ILogger logger, Container container)
   {
      ToolModeOptions toolModeOptions = new();
      commandLineArguments.ApplyTo(toolModeOptions);

      Type? toolModeType = ToolModeUtility.GetToolModeType(toolModeOptions.ToolMode);
      if (toolModeType == null)
      {
         OnInvalidToolMode(logger, toolModeOptions);
         return;
      }

      ToolMode toolMode = (ToolMode)container.Instantiate(toolModeType);

      try
      {
         toolMode.Execute();
      }
      catch (Exception exception)
      {
         Console.WriteLine("An error occurred while executing the tool.");
         logger.LogException(nameof(UndefinedBuildTool), LogLevel.Error, exception);
      }
   }

   private static void OnInvalidToolMode(ILogger logger, ToolModeOptions toolModeOptions)
   {
      logger.Log(nameof(UndefinedBuildTool), LogLevel.Error, $"Invalid tool mode: {toolModeOptions.ToolMode}");

      Console.WriteLine("Invalid tool mode. Available tool modes:");
      Console.ForegroundColor = ConsoleColor.Yellow;
      foreach (string modeOption in ToolModeUtility.GetAllToolModeNames())
      {
         Console.WriteLine($"  {modeOption}");
      }

      Console.ResetColor();
   }
}
