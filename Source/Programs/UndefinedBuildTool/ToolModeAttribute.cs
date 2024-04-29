using System;

namespace UndefinedBuildTool;

[AttributeUsage(AttributeTargets.Class)]
public class ToolModeAttribute : Attribute
{
   public string Name { get; }

   public ToolModeAttribute(string name)
   {
      Name = name;
   }
}
