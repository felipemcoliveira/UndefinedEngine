using System;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[Serializable]
public class IllFormedCodeException : Exception
{
   public int Position { get; }

   public IllFormedCodeException(int position)
   {
      Position = position;
   }

   public IllFormedCodeException(int position, string message)
      : base(message)
   {
      Position = position;
   }
}
