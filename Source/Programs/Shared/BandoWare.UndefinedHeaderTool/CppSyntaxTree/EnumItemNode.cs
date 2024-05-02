using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("UENUM Item {Name,nq}")]
public class EnumItemNode(StringView name) : SyntaxNode
{
   public StringView Name { get; private set; } = name;
}
