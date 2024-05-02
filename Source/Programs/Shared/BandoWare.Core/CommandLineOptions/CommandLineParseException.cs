using System;

namespace BandoWare.Core;

// parse exception 
public class CommandLineParseException : Exception
{
   public CommandLineParseException(string message) : base(message)
   {
   }
}
