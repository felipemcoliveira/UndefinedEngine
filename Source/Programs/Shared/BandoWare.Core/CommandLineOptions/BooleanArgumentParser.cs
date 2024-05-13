using System;

namespace BandoWare.Core;

public class BooleanArgumentParser : CommandArgsParser
{
   public override object ParseArgs(CommandLineArguments commandLineArguments, ReadOnlySpan<string> args, Type targetType)
   {
      if (args.Length > 1)
      {
         throw new CommandLineParseException("Too many arguments.");
      }

      string arg = args[0];
      if (arg.Equals("0"))
      {
         return false;
      }

      if (arg.Equals("1"))
      {
         return true;
      }

      return bool.Parse(arg);
   }
}
