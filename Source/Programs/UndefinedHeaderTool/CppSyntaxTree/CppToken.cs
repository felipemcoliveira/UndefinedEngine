namespace UndefinedHeader.SyntaxTree;

public struct CppToken(int startPosition, int length, CppTokenType type)
{
   public int StartPosition = startPosition;
   public int Length = length;
   public CppTokenType Type = type;
}
