using BandoWare.Core.FileSystem;
using System;

namespace BandoWare.Core;

public class FileReferenceParser : CommandArgsParser
{
   public override object ParseArgs(CommandLineArguments commandLineArguments, ReadOnlySpan<string> args, Type targetType)
   {
      if (args.Length > 1)
      {
         throw new CommandLineParseException("Too many arguments.");
      }

      try
      {
         FileReference fileReference = new(args[0]);
         return fileReference;
      }
      catch
      {

         throw new CommandLineParseException("Invalid file path.");
      }
   }
}

public class DirectoryReferenceParser : CommandArgsParser
{
   public override object ParseArgs(CommandLineArguments commandLineArguments, ReadOnlySpan<string> args, Type targetType)
   {
      if (args.Length > 1)
      {
         throw new CommandLineParseException("Too many arguments.");
      }

      try
      {
         DirectoryReference directoryReference = new(args[0]);
         return directoryReference;
      }
      catch
      {

         throw new CommandLineParseException("Invalid directory path.");
      }
   }
}
