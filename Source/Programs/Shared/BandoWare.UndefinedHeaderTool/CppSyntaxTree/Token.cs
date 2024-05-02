using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct Token(TokenType type, int contentPosition, StringView contentView, int index)
{
   public StringView ValueView { get; private set; } = contentView;
   public int ContentPosition { get; private set; } = contentPosition;
   public TokenType Type { get; private set; } = type;
   public int Index { get; private set; } = index;

   [DebuggerBrowsable(DebuggerBrowsableState.Never)]
   internal readonly string DebuggerDisplay => $"\"{ValueView}\" ({Type} @ {ContentPosition})";
}
