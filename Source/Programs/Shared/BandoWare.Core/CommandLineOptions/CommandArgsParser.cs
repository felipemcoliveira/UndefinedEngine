using System;

namespace BandoWare.Core;

public abstract class CommandArgsParser
{
   public CommandTarget Target { get; set; } = null!;

   public abstract object ParseArgs(CommandLineArguments commandLineArguments, ReadOnlySpan<string> args, Type targetType);
}
