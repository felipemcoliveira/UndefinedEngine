using BandoWare.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BandoWare.UndefinedBuildTool;

[ToolMode("Help")]
public class HelpToolMode(CommandLineArguments commandLineArguments) : ToolMode
{
   private static readonly ValueUsageFormatHandler[] s_ValueUsageFormatHandlers =
   [
      new FunctionValueUsageFormatHandler(),
      new EnumValueUsageFormatHandler()
   ];

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


      if (argumentTarget.Attribute.Description == null)
      {
         Console.WriteLine("    No description provided.");
      }
      else
      {
         Console.WriteLine($"    Description: {argumentTarget.Attribute.Description}");
      }

      Console.WriteLine();
      Console.ResetColor();
   }

   private static string FormatValueUsage(CommandTarget argumentTarget)
   {
      string valueUsage = argumentTarget.Attribute.ValueUsage!;
      int position = 0;

      TextConsumer textConsumer = new(ref position, valueUsage);
      if (TryGetFormattedValueUsage(argumentTarget, ref textConsumer, out string? formattedValueUsage))
      {
         return $"<{formattedValueUsage}>";
      }

      return valueUsage;
   }

   private static bool TryGetFormattedValueUsage
   (
      CommandTarget argumentTarget,
      ref TextConsumer textConsumer,
      [NotNullWhen(true)] out string? output
   )
   {
      if (!textConsumer.TryConsume('$'))
      {
         output = null;
         return false;
      }

      output = null!;
      bool isValidIdentifier = false;
      foreach (ValueUsageFormatHandler handler in s_ValueUsageFormatHandlers)
      {
         if (textConsumer.TryConsume(handler.Identifier))
         {
            isValidIdentifier = true;
            try
            {
               output = handler.Format(argumentTarget, ref textConsumer)!;

            }
            catch (Exception e)
            {
               output = $"FORMAT_ERROR({e.Message})";
               throw;
            }

            break;
         }
      }

      if (!isValidIdentifier)
      {
         throw new InvalidOperationException($"Invalid value usage format: Unknown format at position {textConsumer.Position}");
      }

      if (!textConsumer.IsEndOfText)
      {
         throw new InvalidOperationException($"Invalid value usage format: Unexpected characters at position {textConsumer.Position}");
      }

      return true;
   }
}

public abstract class ValueUsageFormatHandler
{
   public abstract string Identifier { get; }
   public abstract string Format(CommandTarget commandTarget, ref TextConsumer textConsumer);
}

public class FunctionValueUsageFormatHandler : ValueUsageFormatHandler
{
   public override string Identifier => "Function";

   public override string Format(CommandTarget commandTarget, ref TextConsumer textConsumer)
   {
      Type type = TypeUtility.GetUderlyingType(commandTarget.Type);

      if (!textConsumer.TryConsume(':'))
      {
         throw new InvalidOperationException($"Invalid value usage format: Expected ':' at position {textConsumer.Position}");
      }

      if (!textConsumer.TryConsumeIdentifier(out string? methodName))
      {
         throw new InvalidOperationException($"Invalid value usage format: Expected method name at position {textConsumer.Position}");
      }

      MethodInfo? method = commandTarget.MemberInfo.DeclaringType!.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
      if (method == null)
      {
         throw new InvalidOperationException($"Failed to find method {methodName} in type {type.Name}");
      }

      if (method.ReturnType != typeof(string))
      {
         throw new InvalidOperationException($"Method {methodName} in type {type} does not return a string.");
      }

      ParameterInfo[] parameters = method.GetParameters();
      if (parameters.Length == 0)
      {
         return (string)method.Invoke(null, null)!;
      }

      if (parameters.Length > 1)
      {
         throw new InvalidOperationException($"Method {methodName} in type {type} has too many parameters.");
      }

      if (parameters[0].ParameterType != typeof(CommandTarget))
      {
         throw new InvalidOperationException($"Method {methodName} in type {type} first parameter is not of type {nameof(CommandTarget)}.");
      }

      return (string)method.Invoke(null, new object[] { commandTarget })!;
   }
}

public class EnumValueUsageFormatHandler : ValueUsageFormatHandler
{
   public override string Identifier => "EnumValues";

   public override string Format(CommandTarget commandTarget, ref TextConsumer textConsumer)
   {
      Type type = TypeUtility.GetUderlyingType(commandTarget.Type);

      if (!type.IsEnum)
      {
         throw new InvalidOperationException($"Invalid value usage format: EnumValues can only be used with enum types.");
      }

      StringBuilder output = new();
      string[] names = Enum.GetNames(type);
      int outputStart = output.Length;
      return string.Join(" | ", names.Select(v => $"\"{v}\""));
   }
}
