using System;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[Serializable]
public class IllFormedCodeException : Exception
{
   public SourceFilePosition Position { get; }

   public IllFormedCodeException(SourceFilePosition position, string message)
      : base(message)
   {
      Position = position;
   }
}
