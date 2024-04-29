using System;
using System.Text;
using UndefinedCore;

namespace UndefinedBuildTool;

internal class UndefinedBuildTool
{
   public static void Main(string[] args)
   {
      Console.Title = nameof(UndefinedBuildTool);
      CommandLineArguments commandLineArguments;
      try
      {
         commandLineArguments = new(["-mode", "Buildd"]);
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

   private static string PascalCaseToSpacedString(string pascalCase)
   {
      StringBuilder builder = new();
      for (int i = 0; i < pascalCase.Length; i++)
      {
         if (char.IsUpper(pascalCase[i]) && i > 0)
         {
            builder.Append(' ');
         }

         builder.Append(pascalCase[i]);
      }

      return builder.ToString();
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
