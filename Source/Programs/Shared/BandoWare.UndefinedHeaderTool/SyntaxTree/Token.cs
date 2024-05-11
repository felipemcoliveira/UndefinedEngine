using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public struct Token(TokenType type, int textPosition, StringView text)
{
   public readonly int TextEndPosition => TextPosition + Text.Length;

   public StringView Text { get; private set; } = text;
   public int TextPosition { get; private set; } = textPosition;
   public TokenType Type { get; private set; } = type;

   [DebuggerBrowsable(DebuggerBrowsableState.Never)]
   internal readonly string DebuggerDisplay => $"{Text} [{Type} @ {TextPosition}]";
}
