namespace UndefinedHeader.SyntaxTree;

public enum CppTokenType
{
   Unknown = 0,
   Identifier = 1 << 0,
   Keyword = 1 << 1,
   Symbol = 1 << 2,
   StringLiteral = 1 << 3,
   CharacterLiteral = 1 << 4,
   NumericLiteral = 1 << 5,
   EngineHeader = 1 << 6,
   BooleanLiteral = 1 << 7,
   PointerLiteral = 1 << 8,
   EndOfFile = 1 << 9
}
