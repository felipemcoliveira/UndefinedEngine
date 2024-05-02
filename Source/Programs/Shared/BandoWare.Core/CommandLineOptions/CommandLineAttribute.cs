using System;

namespace BandoWare.Core;

[System.AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public sealed class CommandLineAttribute : Attribute
{
   public string Name { get; }
   public string? Description { get; set; }
   public string? Value { get; set; }
   public string? ValueUsage { get; set; }

   private CommandLineAttribute()
   {
      Name = string.Empty;
   }

   public CommandLineAttribute(string name)
   {
      Name = name;
   }
}
