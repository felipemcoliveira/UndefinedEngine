using System;

namespace UndefinedCore;

// parse exception 
public class CommandLineParseException : Exception
{
   public CommandLineParseException(string message) : base(message)
   {
   }
}
