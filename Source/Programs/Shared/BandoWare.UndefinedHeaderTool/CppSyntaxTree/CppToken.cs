using BandoWare.Core;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

public struct CppToken(CppTokenType type, int startPosition, StringView contentView, int index)
{
   public StringView ValueView { get; private set; } = contentView;
   public int StartPosition { get; private set; } = startPosition;
   public CppTokenType Type { get; private set; } = type;
   public int Index { get; private set; } = index;
}
