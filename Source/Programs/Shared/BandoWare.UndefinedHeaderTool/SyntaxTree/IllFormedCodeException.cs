using System;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[Serializable]
public class IllFormedCodeException : Exception
{
   public SourceFileTextPosition Position { get; }

   public IllFormedCodeException(SourceFileTextPosition position, string message)
      : base(message)
   {
      Position = position;
   }
}
