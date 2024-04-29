using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using UndefinedCore;

using Version = UndefinedCore.Version;

namespace UndefinedBuildTool;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public class NativeTypeAttribute : Attribute
{
   public Guid ClassGuid { get; }

   public NativeTypeAttribute(string classGuid)
   {
      ClassGuid = new Guid(classGuid);
   }
}

public class NativeTypes
{
   private static Dictionary<Guid, Type>? s_NativeTypes;

   public static bool TryGetType(Guid classGuid, [NotNullWhen(true)] out Type? type)
   {
      if (s_NativeTypes == null)
      {
         s_NativeTypes = [];
         foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
         {
            foreach (Type t in assembly.GetTypes())
            {
               if (t.GetCustomAttribute<NativeTypeAttribute>() is NativeTypeAttribute nativeTypeAttribute)
               {
                  s_NativeTypes.Add(nativeTypeAttribute.ClassGuid, t);
               }
            }
         }
      }

      return s_NativeTypes.TryGetValue(classGuid, out type);

   }
}

[NativeType("dbca19a3-bb1d-4d0e-9cf9-230522850996")]
public class Project
{
   public string Name { get; set; } = string.Empty;
   public string FilePath { get; set; } = string.Empty;
}

public static class BinaryReaderExtensions
{
   public static Guid ReadGuid(this BinaryReader reader)
   {
      long part1 = reader.ReadInt64();
      long part2 = reader.ReadInt64();
      Span<byte> guidBytes = stackalloc byte[16];
      BitConverter.TryWriteBytes(guidBytes, part1);
      BitConverter.TryWriteBytes(guidBytes[8..], part2);
      return new Guid(guidBytes);
   }
}

public class ProjectWindowSettings
{
   public Version? VSCompilerVersion { get; set; }
}

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
