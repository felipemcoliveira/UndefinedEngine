using BandoWare.Core;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public struct Token(TokenType type, int startPosition, StringView contentView, int index)
{
   public StringView ValueView { get; private set; } = contentView;
   public int StartPosition { get; private set; } = startPosition;
   public TokenType Type { get; private set; } = type;
   public int Index { get; private set; } = index;
}
