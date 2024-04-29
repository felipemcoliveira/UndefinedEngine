using System;

namespace UndefinedHeader.SyntaxTree;

[Serializable]
public class CppIllFormedCodeException : Exception
{
   public int Position { get; }

   public CppIllFormedCodeException(int position)
   {
      Position = position;
   }

   public CppIllFormedCodeException(int position, string message)
      : base(message)
   {
      Position = position;
   }
}
