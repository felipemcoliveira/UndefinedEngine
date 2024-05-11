using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("{Type,nq} HEADER")]
public class HeaderNode(StringView type) : Node
{
   public StringView Type { get; private set; } = type;
}
