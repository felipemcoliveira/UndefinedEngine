using System;

namespace BandoWare.Core;

public class ArrayArgumentParser : CommandArgsParser
{
   public override object ParseArgs(CommandLineArguments commandLineArguments, ReadOnlySpan<string> args, Type targetType)
   {

      Type? elementType = targetType.GetElementType();
      if (elementType == null)
      {
         throw new InvalidOperationException("Invalid target type.");
      }

      elementType = TypeUtility.GetUderlyingType(elementType);

      object[] array = new object[args.Length];
      for (int i = 0; i < args.Length; i++)
      {
         array[i] = commandLineArguments.ParseArgs(Target, args.Slice(i, 1), elementType);
      }

      return array;
   }
}
