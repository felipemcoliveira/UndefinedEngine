using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UndefinedBuildTool;

public static class ToolModeUtility
{
   public static string[] GetAllToolModeNames()
   {
      return GetAllToolModeTypes().Select(v => v.ToolModeAttribute.Name).ToArray();
   }

   public static Type? GetToolModeType(string name)
   {
      foreach ((Type type, ToolModeAttribute toolModeAttribute) in GetAllToolModeTypes())
      {
         if (toolModeAttribute.Name == name)
         {
            return type;
         }
      }

      return null;
   }

   public static IEnumerable<(Type Type, ToolModeAttribute ToolModeAttribute)> GetAllToolModeTypes()
   {
      foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
      {
         if (type.GetCustomAttribute<ToolModeAttribute>() is ToolModeAttribute toolModeAttribute)
         {
            yield return (type, toolModeAttribute);
         }
      }
   }
}