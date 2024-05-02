using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BandoWare.Core;


public class CommandTarget(string command, Action<object?, object?> setValue, CommandLineAttribute attribute, MemberInfo memberInfo, Type type)
{
   public string CommandName { get; } = command;

   public Action<object?, object?> SetValue { get; } = setValue;

   public Type Type { get; } = type;

   public MemberInfo MemberInfo { get; } = memberInfo;

   public CommandLineAttribute Attribute { get; } = attribute;
}

public class ArgumentValue(string rawValue, int position)
{
   public string RawValue { get; } = rawValue;
   public int Position { get; } = position;
}

public class CommandLineArguments
{
   public string[] RawArguments { get; }
   public List<CommandTarget> CommandTargets { get; } = [];

   private static readonly Dictionary<Type, ArgumentParser> s_DefaultValueParsers =
      CreateDefaultArgumentParsers();

   private static readonly Regex s_DefaultValueConsumeRegex = new("^(?!-[a-zA-Z])");

   private readonly Dictionary<string, int> m_CommandPosition = new(StringComparer.OrdinalIgnoreCase);
   private Dictionary<string, CommandTarget> m_CommandTargetByCommandName = new(StringComparer.OrdinalIgnoreCase);
   private Dictionary<Type, List<CommandTarget>> m_CommandTargetByType = [];

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
         if (m_CommandTargetByType.TryGetValue(type, out List<CommandTarget>? argumentTargets))
         {
            foreach (CommandTarget argumentTarget in argumentTargets)
            {
               if (!m_CommandPosition.TryGetValue(argumentTarget.CommandName, out int position))
               {
                  continue;
               }

               ArgumentParser argumentParser = GetArgumentParser(argumentTarget.Type);

               if (argumentTarget.Attribute.ValueUsage != null)
               {
                  int nextPosition = position + 1;
                  object? value = argumentParser.Parse(RawArguments[nextPosition], argumentTarget.MemberInfo);
                  argumentTarget.SetValue(target, value);
                  continue;
               }

               if (argumentTarget.Attribute.Value != null)
               {
                  object? value = argumentParser.Parse(argumentTarget.Attribute.Value, argumentTarget.MemberInfo);
                  argumentTarget.SetValue(target, value);
                  continue;
               }

               else if (argumentTarget.Type == typeof(bool))
               {
                  argumentTarget.SetValue(target, true);
               }

               throw new InvalidOperationException("Invalid target.");
            }
         }
      }
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

      if (!m_CommandTargetByType.TryGetValue(containingType, out List<CommandTarget>? commandTargetByName))
      {
         commandTargetByName = [];
         m_CommandTargetByType.Add(containingType, commandTargetByName);
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

   private static ArgumentParser GetArgumentParser(Type targetType)
   {
      ArgumentNullException.ThrowIfNull(targetType, nameof(targetType));

      Type? underlyingType = Nullable.GetUnderlyingType(targetType);
      targetType = underlyingType ?? targetType;

      if (targetType.IsEnum)
      {
         return s_DefaultValueParsers[typeof(Enum)];
      }

      if (s_DefaultValueParsers.TryGetValue(targetType, out ArgumentParser? parser))
      {
         return parser;
      }

      throw new NotImplementedException();
   }

   private static Dictionary<Type, ArgumentParser> CreateDefaultArgumentParsers()
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
         [typeof(Enum)] = new EnumArgumentParser()
      };
      ;
   }

   private void Parse(string[] args)
   {
      for (int i = 0; i < args.Length; i++)
      {
         if (!IsCommandIdentifier(args[i]))
         {
            throw new CommandLineParseException($"Argument '{args[i]}' is not a command.");
         }

         if (!m_CommandTargetByCommandName.TryGetValue(args[i], out CommandTarget? commandTarget) || commandTarget == null)
         {
            throw new CommandLineParseException($"Command '{args[i]}' does not exist.");
         }

         m_CommandPosition.Add(args[i], i);

         if (commandTarget.Attribute.ValueUsage == null)
         {
            continue;
         }

         bool hasNext = i + 1 < args.Length;
         if ((!hasNext || IsCommandIdentifier(args[i + 1])) && commandTarget.Attribute.Value == null)
         {
            throw new CommandLineParseException($"Missing command '{args[i]}' value.");
         }

         i++;
      }
   }

   public static bool IsCommandIdentifier(string command)
   {
      return command.StartsWith('-') && command.Length > 1 && char.IsLetter(command[1]);
   }
}
