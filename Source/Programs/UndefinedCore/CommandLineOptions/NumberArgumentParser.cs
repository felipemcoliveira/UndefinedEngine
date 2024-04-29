using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace UndefinedCore;

[System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
sealed class CommandLineArgumentNumberStyleAttribute : Attribute
{
   public NumberStyles Style { get; }

   public CommandLineArgumentNumberStyleAttribute(NumberStyles style)
   {
      Style = style;
   }
}

public class NumberArgumentParser : ArgumentParser
{
   private static Dictionary<Type, Func<string, NumberStyles, object>> s_ParseFunctions = new()
   {
      { typeof(int), (arg, style) => int.Parse(arg, style) },
      { typeof(uint), (arg, style) => uint.Parse(arg, style) },
      { typeof(short), (arg, style) => short.Parse(arg, style) },
      { typeof(ushort), (arg, style) => ushort.Parse(arg, style) },
      { typeof(long), (arg, style) => long.Parse(arg, style) },
      { typeof(ulong), (arg, style) => ulong.Parse(arg, style) },
      { typeof(byte), (arg, style) => byte.Parse(arg, style) },
      { typeof(sbyte), (arg, style) => sbyte.Parse(arg, style) },
      { typeof(float), (arg, style) => float.Parse(arg, style) },
      { typeof(double), (arg, style) => double.Parse(arg, style) },
      { typeof(decimal), (arg, style) => decimal.Parse(arg, style) }
   };

   public override object Parse(string arg, MemberInfo targetMember)
   {
      CommandLineArgumentNumberStyleAttribute? styleArgment
         = targetMember.GetCustomAttribute<CommandLineArgumentNumberStyleAttribute>();

      NumberStyles style = styleArgment?.Style ?? NumberStyles.Integer;

      Type targetType = GetMemberTypeWithoutNullable(targetMember);
      if (!s_ParseFunctions.TryGetValue(targetType, out Func<string, NumberStyles, object>? parseFunction))
      {
         throw new InvalidOperationException("Invalid target type.");
      }

      return parseFunction!(arg, style);
   }
}
