using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("{Name,nq}")]
public class HeaderNode(StringView type) : SyntaxNode
{
   public StringView Type { get; private set; } = type;
}
