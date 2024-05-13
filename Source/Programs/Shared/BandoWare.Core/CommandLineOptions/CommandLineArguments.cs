using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace BandoWare.Core;

public record CommandTarget
(
   string CommandName,
   Action<object?, object?> SetValue,
   CommandLineAttribute Attribute,
   MemberInfo MemberInfo,
   Type Type
);

public class ParsedCommand(CommandTarget commandTarget)
{
   public CommandTarget CommandTarget { get; } = commandTarget;
   public List<string> Arguments { get; } = [];

   public int ArgumentCount => Arguments.Count;
}

public class CommandLineArguments
{
   public string[] RawArguments { get; }
   public List<CommandTarget> CommandTargets { get; } = [];

   private static readonly Dictionary<Type, CommandArgsParser> s_DefaultValueParsers =
      CreateDefaultArgumentParsers();

   private Dictionary<string, CommandTarget> m_CommandTargetByCommandName = new(StringComparer.OrdinalIgnoreCase);
   private Dictionary<Type, List<CommandTarget>> m_CommandTargetsByType = [];
   private Dictionary<CommandTarget, ParsedCommand> m_ParsedCommands = [];

   public CommandLineArguments(string[] arguments)
   {
      RawArguments = arguments;

      PopulateCommandTargets();
      Parse(arguments);
   }

   public void ApplyTo(object target)
   {
      for (Type type = target.GetType(); type != typeof(object); type = type.BaseType!)
      {
         if (m_CommandTargetsByType.TryGetValue(type, out List<CommandTarget>? argumentTargets))
         {
            foreach (CommandTarget commandTarget in argumentTargets)
            {
               if (!m_ParsedCommands.TryGetValue(commandTarget, out ParsedCommand? parsedCommand))
               {
                  continue;
               }

               if (commandTarget.Attribute.Value != null)
               {
                  if (parsedCommand.ArgumentCount > 0)
                  {
                     throw new CommandLineParseException($"Command '{commandTarget.CommandName}' is not expecting any arguments.");
                  }

                  commandTarget.SetValue(target, commandTarget.Attribute.Value);
                  continue;
               }

               if (parsedCommand.ArgumentCount == 0)
               {
                  if (commandTarget.Type != typeof(bool) && commandTarget.Type != typeof(bool?))
                  {
                     throw new CommandLineParseException($"Missing argument for command '{commandTarget.CommandName}'.");
                  }

                  commandTarget.SetValue(commandTarget, true);
               }

               string[] args = parsedCommand.Arguments.ToArray();
               object value = ParseArgs(commandTarget, args, commandTarget.Type);
               commandTarget.SetValue(target, value);
            }
         }
      }
   }

   public object ParseArgs(CommandTarget commandTarget, ReadOnlySpan<string> args, Type targetType)
   {
      CommandArgsParser argumentParser = GetArgumentParser(commandTarget.Type);
      argumentParser.Target = commandTarget;

      return argumentParser.ParseArgs(this, args, targetType);
   }

   private void AddCommandTarget(CommandTarget commandTarget, Type containingType)
   {
      if (commandTarget.Attribute.Value == null && commandTarget.Attribute.ValueUsage == null)
      {
         if (commandTarget.Type != typeof(bool))
         {
            // TODO: add proper message
            throw new InvalidOperationException();
         }
      }

      if (!m_CommandTargetsByType.TryGetValue(containingType, out List<CommandTarget>? commandTargetByName))
      {
         commandTargetByName = [];
         m_CommandTargetsByType.Add(containingType, commandTargetByName);
      }

      commandTargetByName.Add(commandTarget);
      CommandTargets.Add(commandTarget);
      m_CommandTargetByCommandName.Add(commandTarget.CommandName, commandTarget);
   }

   private void PopulateCommandTargets()
   {
      Assembly executingAssembly = Assembly.GetExecutingAssembly();
      PopulateCommandTargets(executingAssembly);

      Assembly? entryAssembly = Assembly.GetEntryAssembly();
      if (entryAssembly != executingAssembly && entryAssembly != null)
      {
         PopulateCommandTargets(entryAssembly);
      }
   }

   private void PopulateCommandTargets(Assembly assembly)
   {
      foreach (Type type in assembly.GetTypes())
      {
         foreach (FieldInfo memberInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
         {
            foreach (CommandLineAttribute attribute in memberInfo.GetCustomAttributes<CommandLineAttribute>())
            {
               AddCommandTarget(new(attribute.Name, memberInfo.SetValue, attribute, memberInfo, memberInfo.FieldType), type);
            }
         }

         foreach (PropertyInfo memberInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
         {
            foreach (CommandLineAttribute attribute in memberInfo.GetCustomAttributes<CommandLineAttribute>())
            {
               AddCommandTarget(new(attribute.Name, memberInfo.SetValue, attribute, memberInfo, memberInfo.PropertyType), type);
            }
         }
      }
   }

   public static CommandArgsParser GetArgumentParser(Type targetType)
   {
      ArgumentNullException.ThrowIfNull(targetType, nameof(targetType));
      targetType = TypeUtility.GetUderlyingType(targetType);

      if (targetType.IsEnum)
      {
         return s_DefaultValueParsers[typeof(Enum)];
      }

      if (targetType.IsArray)
      {
         return s_DefaultValueParsers[typeof(Array)];
      }

      if (s_DefaultValueParsers.TryGetValue(targetType, out CommandArgsParser? parser))
      {
         return parser;
      }

      throw new NotImplementedException();
   }

   private static Dictionary<Type, CommandArgsParser> CreateDefaultArgumentParsers()
   {
      NumberArgumentParser numberArgumentParser = new();

      return new()
      {
         [typeof(int)] = numberArgumentParser,
         [typeof(uint)] = numberArgumentParser,
         [typeof(short)] = numberArgumentParser,
         [typeof(ushort)] = numberArgumentParser,
         [typeof(long)] = numberArgumentParser,
         [typeof(ulong)] = numberArgumentParser,
         [typeof(byte)] = numberArgumentParser,
         [typeof(sbyte)] = numberArgumentParser,
         [typeof(float)] = numberArgumentParser,
         [typeof(double)] = numberArgumentParser,
         [typeof(decimal)] = numberArgumentParser,
         [typeof(string)] = new StringArgumentParser(),
         [typeof(bool)] = new BooleanArgumentParser(),
         [typeof(Enum)] = new EnumArgumentParser(),
         [typeof(Array)] = new ArrayArgumentParser()
      };
   }

   private void Parse(string[] args)
   {
      int argIndex = 0;
      ParsedCommand? lastCommandSpec = null;
      m_ParsedCommands = [];

      while (argIndex < args.Length)
      {
         if (TryConsumeCommand(args[argIndex], out CommandTarget? target))
         {
            lastCommandSpec = new ParsedCommand(target);
            m_ParsedCommands.Add(target, lastCommandSpec);
            argIndex++;
            continue;
         }

         if (lastCommandSpec == null)
         {
            throw new CommandLineParseException($"Invalid command '{args[argIndex]}'.");
         }

         lastCommandSpec.Arguments.Add(args[argIndex]);
         argIndex++;
      }
   }

   public bool TryConsumeCommand(string arg, [NotNullWhen(true)] out CommandTarget? target)
   {
      int position = 0;
      TextConsumer textConsumer = new(ref position, arg);
      if (!textConsumer.TryConsume('-') || !textConsumer.TryConsumeIdentifier(IdentifierStyle.AlphanumericWithDash))
      {
         target = null;
         return false;
      }

      if (!textConsumer.IsEndOfText)
      {
         throw new CommandLineParseException($"Invalid command '{arg}'.");
      }

      if (!m_CommandTargetByCommandName.TryGetValue(arg, out target))
      {
         return false;
      }

      return true;
   }
}
