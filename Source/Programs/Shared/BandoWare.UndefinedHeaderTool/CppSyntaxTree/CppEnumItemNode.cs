using BandoWare.Core;
using System.Diagnostics;

namespace BandoWare.UndefinedHeaderTool.SyntaxTree;

[DebuggerDisplay("UENUM Item {Name,nq}")]
public class CppEnumItemNode(StringView name) : CppSyntaxNode
{
   public StringView Name { get; private set; } = name;
}
