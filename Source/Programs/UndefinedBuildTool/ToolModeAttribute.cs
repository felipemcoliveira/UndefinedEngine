using System;

namespace BandoWare.UndefinedBuildTool;

[AttributeUsage(AttributeTargets.Class)]
public class ToolModeAttribute(string name) : Attribute
{
   public string Name { get; } = name;
}
