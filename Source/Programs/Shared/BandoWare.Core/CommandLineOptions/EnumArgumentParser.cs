using System;

namespace BandoWare.Core;

public class EnumArgumentParser : CommandArgsParser
{
   public override object ParseArgs(CommandLineArguments commandLineArguments, ReadOnlySpan<string> args, Type targetType)
   {
      if (args.Length > 1)
      {
         throw new CommandLineParseException("Too many arguments.");
      }

      return Enum.Parse(targetType, args[0], true);
   }
}
