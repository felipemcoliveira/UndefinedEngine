using System;

namespace BandoWare.Core;

public class StringArgumentParser : CommandArgsParser
{
   public override object ParseArgs(CommandLineArguments commandLineArguments, ReadOnlySpan<string> args, Type targetType)
   {
      if (args.Length > 1)
      {
         throw new CommandLineParseException($"Too many arguments for command.");
      }

      return args[0];
   }
}
