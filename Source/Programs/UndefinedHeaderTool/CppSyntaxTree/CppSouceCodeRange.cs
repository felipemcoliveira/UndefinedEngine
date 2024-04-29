namespace UndefinedHeader.SyntaxTree;

public readonly struct CppSouceCodeRange(int startPosition, int line, int column)
{
   public int StartPosition { get; } = startPosition;
   public int Line { get; } = line;
   public int Column { get; } = column;
}
