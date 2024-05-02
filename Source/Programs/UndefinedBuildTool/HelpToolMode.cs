using BandoWare.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BandoWare.UndefinedBuildTool;

[ToolMode("Help")]
public class HelpToolMode(CommandLineArguments commandLineArguments) : ToolMode
{
   public override void Execute()
   {
      Console.WriteLine("Usage:");
      IEnumerable<CommandTarget> commandTargs = commandLineArguments.CommandTargets.OrderBy(t => t.CommandName);
      foreach (CommandTarget argumentTarget in commandTargs)
      {
         PrintArgumentTarget(argumentTarget);
      }
   }

   private static void PrintArgumentTarget(CommandTarget argumentTarget)
   {
      Console.ForegroundColor = ConsoleColor.Gray;
      Console.Write("  " + argumentTarget.CommandName);
      if (argumentTarget.Attribute.ValueUsage != null)
      {
         Console.ForegroundColor = ConsoleColor.Yellow;
         string valueUsage = FormatValueUsage(argumentTarget);
         Console.Write($" {valueUsage}");
      }

      Console.WriteLine();
      Console.ResetColor();

      Console.ForegroundColor = ConsoleColor.DarkGray;

      string description = argumentTarget.Attribute.Description ?? "No description provided.";
      Console.WriteLine($"    Description: {description}");

      Console.WriteLine();
      Console.ResetColor();
   }

   private static string FormatValueUsage(CommandTarget argumentTarget)
   {
      string valueUsage = argumentTarget.Attribute.ValueUsage!;
      if (valueUsage[0] == '$')
      {
         if (valueUsage == "$EnumValues")
            return FormatEnumValues(argumentTarget);
         else if (valueUsage.StartsWith("$StaticField:"))
         {
            return FormatStaticFieldOptions(argumentTarget, valueUsage);
         }

         throw new InvalidOperationException($"Invalid value usage: {valueUsage}");
      }

      return valueUsage;
   }

   private static string FormatStaticFieldOptions(CommandTarget argumentTarget, string valueUsage)
   {
      string[] parts = valueUsage.Split(':');
      string fieldName = parts[1];
      Type type = argumentTarget.MemberInfo.DeclaringType!;
      FieldInfo? field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
      if (field == null)
         throw new InvalidOperationException($"Failed to find static field {fieldName} in type {type.Name}");

      if (field.FieldType != typeof(string[]))
         throw new InvalidOperationException($"Static field {fieldName} in type {type} is not an array of string.");

      string[] values = (string[])field.GetValue(null)!;
      return $"<{string.Join(" | ", values.Select(v => $"\"{v}\""))}>";
   }

   private static string FormatEnumValues(CommandTarget argumentTarget)
   {
      string[] names = Enum.GetNames(TypeUtility.GetUderlyingType(argumentTarget.Type));
      return $"<{string.Join(" | ", names.Select(v => $"\"{v}\""))}>";
   }
}
